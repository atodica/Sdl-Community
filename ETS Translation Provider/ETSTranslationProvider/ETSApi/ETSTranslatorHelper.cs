﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using ETSLPConverter;
using ETSTranslationProvider.Helpers;
using Newtonsoft.Json;
using Sdl.Community.Toolkit.LanguagePlatform.XliffConverter;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace ETSTranslationProvider.ETSApi
{
	public enum APIVersion { Unknown, v1, v2 };

	public static class ETSTranslatorHelper
	{
		private static Func<Uri, HttpClient, HttpResponseMessage> ETSPost = delegate (Uri uri, HttpClient client)
		{ return client.PostAsync(uri, content: null).Result; };
		private static Func<Uri, HttpClient, HttpResponseMessage> ETSGet = delegate (Uri uri, HttpClient client)
		{ return client.GetAsync(uri).Result; };
		private static ETSLanguagePair[] LanguagePairsOnServer;
		private static object languageLock = new object();
		private static object optionsLock = new object();

		private enum ErrorHResult
		{
			HandshakeFailure = -2146232800,
			ServerInaccessible = -2147467259,
			RequestTimeout = -2146233029,
		}

		public static readonly Log Log = Log.Instance;

		/// <summary>
		/// Get the translation of an xliff file using the ETS API.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="languageDirection"></param>
		/// <param name="xliffFile"></param>
		/// <returns></returns>
		public static string GetTranslation(TranslationOptions options,
			LanguagePair languageDirection,
			Xliff xliffFile)
		{
			Log.Logger.Trace("");
			var text = xliffFile.ToString();
			var queryString = HttpUtility.ParseQueryString(string.Empty);
			var encodedInput = text.Base64Encode();

			lock (optionsLock)
			{
				if (options.ApiVersion == APIVersion.Unknown)
				{
					SetETSApiVersion(options);
				}
			}

			if (options.ApiVersion == APIVersion.v1)
			{
				queryString["sourceLanguageId"] = languageDirection.SourceCulture.ToETSCode();
				queryString["targetLanguageId"] = languageDirection.TargetCulture.ToETSCode();
				queryString["text"] = encodedInput;
			}
			else
			{
				// If LPPreferences doesn't contain the target language (source is always the same), figure out the
				// preferred LP. Previously set preferred LPs will stay, so this won't get run each time if you have
				// multiple LPs.
				if (!options.LPPreferences.ContainsKey(languageDirection.TargetCulture))
				{
					options.SetPreferredLanguages(new LanguagePair[] { languageDirection });
					if (!options.LPPreferences.ContainsKey(languageDirection.TargetCulture))
					{
						throw new Exception("There are no language pairs currently accessible via ETS.");
					}
				}
				queryString["languagePairId"] = options.LPPreferences[languageDirection.TargetCulture].LanguagePairId;
				queryString["input"] = encodedInput;
			}
			queryString["inputFormat"] = "application/x-xliff";

			Log.Logger.Debug("Sending translation request for: {0}", encodedInput);
			string jsonResult;
			try
			{
				jsonResult = ContactETSServer(ETSPost, options, "translations/quick", queryString);
			}
			catch (Exception e)
			{
				Log.Logger.Error($"{Constants.Translation}: {e.Message}\n {e.StackTrace}\n Encoded Input: {encodedInput}");
				throw;
			}

			var encodedTranslation = JsonConvert.DeserializeObject<ETSTranslationOutput>(jsonResult).Translation;
			var decodedTranslation = encodedTranslation.Base64Decode();
			Log.Logger.Debug("Resultant translation is: {0}", encodedTranslation);
			return decodedTranslation;
		}

		/// <summary>
		/// Get an array of Language Pairs accessible by the credentials provided.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static ETSLanguagePair[] GetLanguagePairs(TranslationOptions options)
		{
			Log.Logger.Trace("");
			lock (languageLock)
			{
				if (LanguagePairsOnServer == null || !LanguagePairsOnServer.Any())
				{
					try
					{
						// Ideally no exception should be thrown from ContactETSServer, but in rare cases
						// it could successfully authenticate using username/password, but then reject the token. If
						// that happens, open a message with the related error
						var jsonResult = ContactETSServer(ETSGet, options, "language-pairs");
						var languagePairs = JsonConvert.DeserializeObject<LanguagePairResult>(jsonResult).LanguagePairs;
						LanguagePairsOnServer = (languagePairs != null ? languagePairs : new ETSLanguagePair[0]);

						// In 60 seconds, wipe the LPs so we query again. That way, if someone makes a change, we'll
						// pick it up eventually.
						Task.Factory.StartNew(() =>
						{
							System.Threading.Thread.Sleep(60000);
							lock (languageLock)
							{
								ExpireLanguagePairs();
							}
						});
					}
					catch (Exception e)
					{
						Log.Logger.Error($"{Constants.LanguagePairs}: {Constants.InaccessibleLangPairs}:  {e.Message}\n {e.StackTrace}");

						if (Environment.UserInteractive)
						{
							MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
						LanguagePairsOnServer = new ETSLanguagePair[0];
					}
				}
			}
			return LanguagePairsOnServer;
		}

		public static void ExpireLanguagePairs()
		{
			LanguagePairsOnServer = new ETSLanguagePair[0];
		}

		/// <summary>
		/// Queries the ETS server specified in options.
		/// </summary>
		/// <param name="etsHttpMethod">ETSPost or ETSGet</param>
		/// <param name="options">The translation options</param>
		/// <param name="path">The path after /api/vX/ for ets api queries</param>
		/// <param name="parameters">data to include in the query</param>
		/// <param name="useHTTP">Whether to use HTTP over HTTPS</param>
		/// <param name="timesToRetry">number of times to retry the query</param>
		/// <returns>The string result of the request.</returns>
		private static string ContactETSServer(Func<Uri, HttpClient, HttpResponseMessage> etsHttpMethod,
			TranslationOptions options,
			string path,
			NameValueCollection parameters = null,
			bool useHTTP = false,
			int timesToRetry = 5)
		{
			Log.Logger.Trace("");

			lock (optionsLock)
			{
				if (options.ApiVersion == APIVersion.Unknown)
				{
					SetETSApiVersion(options);
				}
			}

			ServicePointManager.Expect100Continue = true;
			ServicePointManager.DefaultConnectionLimit = 9999;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			using (var httpClient = new HttpClient())
			{
				if (options.UseBasicAuthentication)
				{
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
				}
				else
				{
					// Append colon to the api key, but leave it off of the Options variable
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", (options.ApiToken + ":").Base64Encode());
				}

				var builder = new UriBuilder(options.Uri);
				builder.Path = string.Format("/api/{0}/{1}", options.ApiVersionString, path);
				builder.Scheme = useHTTP ? Uri.UriSchemeHttp : Uri.UriSchemeHttps;

				if (parameters != null)
				{
					builder.Query = parameters.ToString();
				}
				HttpResponseMessage httpResponse = null;

				try
				{
					httpResponse = etsHttpMethod(builder.Uri, httpClient);
				}
				catch (Exception e)
				{
					while (e.InnerException != null)
					{
						e = e.InnerException;
					}
					if (!useHTTP && e.HResult == (int)ErrorHResult.HandshakeFailure)
					{
						return ContactETSServer(etsHttpMethod, options, path, parameters, true);
					}
					if (timesToRetry > 0)
					{
						return ContactETSServer(etsHttpMethod, options, path, parameters, useHTTP, --timesToRetry);
					}
					Log.Logger.Error($"{Constants.ETSServerContact}:\n {Constants.ETSServerContactExResult} {e.HResult}\n {e.Message}\n {e.StackTrace}");
					throw TranslateAggregateException(e);
				}

				if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
				{
					return httpResponse.Content.ReadAsStringAsync().Result;
				}
				else if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
				{
					Log.Logger.Error($"{Constants.ETSServerContact}: {Constants.InvalidCredentials}");
					throw new UnauthorizedAccessException("The credentials provided are not authorized.");
				}
				else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
				{
					Log.Logger.Error($"{Constants.ETSServerContact}: {Constants.BadRequest} {0}", httpResponse.Content.ReadAsStringAsync().Result);
					throw new Exception($"There was a problem with the request: { httpResponse.Content.ReadAsStringAsync().Result }");
				}
				Log.Logger.Error($"{Constants.ETSServerContact}: {(int)httpResponse.StatusCode} {Constants.StatusCode}");
				if (timesToRetry > 0)
				{
					return ContactETSServer(etsHttpMethod, options, path, parameters, useHTTP, --timesToRetry);
				}
				return null;
			}
		}

		public static void SetETSApiVersion(TranslationOptions options)
		{
			try
			{
				options.ApiVersion = APIVersion.v2;
				var systemInfo = ContactETSServer(ETSGet, options, "system/info");
				if (systemInfo == null)
				{
					options.ApiVersion = APIVersion.v1;
				}
			}
			catch (Exception e)
			{
				Log.Logger.Error($"{Constants.ETSApiVersion}: {e.Message}\n {e.StackTrace}");
				options.ApiVersion = APIVersion.v1;
			}
		}

		/// <summary>
		/// Verifies that the API Key passed by the user is a valid API key.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="credentials"></param>
		public static void VerifyBasicAPIToken(TranslationOptions options, GenericCredentials credentials)
		{
			Log.Logger.Trace("");
			if (options == null)
			{
				throw new ArgumentNullException("Options is null");
			}
			var oldAPIKey = options.ApiToken;
			options.ApiToken = credentials["API-Key"];
			options.UseBasicAuthentication = credentials["UseApiKey"] != "true";

			try
			{
				// Make a request to the API using whatever path desired.
				ContactETSServer(ETSGet, options, "language-pairs");
			}
			catch (AggregateException e)
			{
				Log.Logger.Error($"{Constants.VerifyBasicAPIToken}: {e.Message}\n {e.StackTrace}");
				throw TranslateAggregateException(e);
			}
			catch (SocketException e)
			{
				Log.Logger.Error($"{Constants.VerifyBasicAPIToken}: {e.Message}\n {e.StackTrace}");
				throw TranslateAggregateException(e);
			}
			finally
			{
				options.ApiToken = oldAPIKey;
			}
		}

		/// <summary>
		/// Using the username and password passed in via credentials, obtain the authentication token that will be
		/// later used to validate API calls.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="credentials"></param>
		/// <param name="useHTTP"></param>
		/// <returns></returns>
		public static string GetAuthToken(
			TranslationOptions options,
			GenericCredentials credentials,
			bool useHTTP = false)
		{
			Log.Logger.Trace("");

			lock (optionsLock)
			{
				if (options.ApiVersion == APIVersion.Unknown)
				{
					SetETSApiVersion(options);
				}
			}

			ServicePointManager.Expect100Continue = true;
			ServicePointManager.DefaultConnectionLimit = 9999;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

			using (var httpClient = new HttpClient())
			{
				// Build the URI for querying the token
				var builder = new UriBuilder(options.Uri);
				builder.Path = string.Format("/api/{0}/auth", options.ApiVersionString);

				// Pass in the username and password as parameters to retrieve the auth token
				var queryString = HttpUtility.ParseQueryString(string.Empty);
				queryString["username"] = credentials.UserName;
				queryString["password"] = credentials.Password;
				builder.Query = queryString.ToString();
				builder.Scheme = useHTTP ? Uri.UriSchemeHttp : Uri.UriSchemeHttps;

				// Users may be hosting the service locally and therefore not sign their certificates. If so,
				// we'll want to accept all certificates. Otherwise, this would throw an exception.
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

				try
				{
					var httpResponse = httpClient.PostAsync(builder.Uri, null).Result;
					if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
					{
						return httpResponse.Content.ReadAsStringAsync().Result;
					}
					else if (httpResponse.StatusCode == HttpStatusCode.Unauthorized || httpResponse.StatusCode == HttpStatusCode.BadRequest)
					{
						throw new UnauthorizedAccessException("The credentials provided are not authorized.");
					}
					throw new Exception("No response from the URI provided");
				}
				catch (Exception e)
				{
					while (e.InnerException != null)
					{
						e = e.InnerException;
					}

					if (!useHTTP && e.HResult == (int)ErrorHResult.HandshakeFailure)
					{
						return GetAuthToken(options, credentials, true);
					}
					Log.Logger.Error($"{Constants.AuthToken}: {e.Message}\n {e.StackTrace}");
					throw TranslateAggregateException(e);
				}
			}
		}

		/// <summary>
		/// Translate exceptions thrown from the http requests into exceptions with client-friendly messages.
		/// </summary>
		/// <param name="culprit"></param>
		/// <returns></returns>
		private static Exception TranslateAggregateException(Exception culprit)
		{
			Log.Logger.Trace("");
			while (culprit.InnerException != null)
			{
				culprit = culprit.InnerException;
			}

			if (culprit.HResult == (int)ErrorHResult.HandshakeFailure)
			{
				return new WebException("You are using an older version of the API that does not support username/password. Please use the API key instead.");
			}
			if (culprit.HResult == (int)ErrorHResult.ServerInaccessible)
			{
				return new WebException("Error with the server information. A connection cannot be formed. Please ensure the server information is correct.");
			}
			// TODO: Cannot replicate this. If possible, convert this to an HResult enum
			if (culprit.Message.Contains("the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond"))
			{
				return new WebException("The host was unable to be reached within an acceptable amount of time. Please ensure you are able to connect to the server from this computer.");
			}
			if (culprit.HResult == (int)ErrorHResult.RequestTimeout)
			{
				return new WebException("The request has been cancelled, either due to timeout or being interrupted externally.");
			}
			Log.Logger.Error($"{Constants.TranslateAggregateException}: {culprit}");
			return culprit;
		}

		#region String encoding extension methods
		/// <summary>
		/// Encode a string using base64 encoding.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private static string Base64Encode(this string text)
		{
			Log.Logger.Trace("");
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
		}

		/// <summary>
		/// Decode a base64 encoded string.
		/// </summary>
		/// <param name="encodedText"></param>
		/// <returns></returns>
		private static string Base64Decode(this string encodedText)
		{
			Log.Logger.Trace("");
			return Encoding.UTF8.GetString(Convert.FromBase64String(encodedText));
		}
		#endregion
	}
}
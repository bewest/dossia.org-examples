/*
 * Copyright 2009 Dossia
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


package org.dossia.oauth;

import java.util.Collection;
import java.util.Map;
import java.util.List;
import java.util.ArrayList;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.FileOutputStream;
import java.io.File;
import java.net.URISyntaxException;
import java.net.URL;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import net.oauth.client.httpclient3.HttpClient3;
import net.oauth.client.OAuthClient;
import net.oauth.http.HttpMessage;
import net.oauth.OAuthServiceProvider;
import net.oauth.OAuthConsumer;
import net.oauth.OAuthAccessor;
import net.oauth.OAuth;
import net.oauth.OAuthMessage;
import net.oauth.OAuthException;


public class DossiaOAuthClient extends OAuthClient
{
    private static String requestUrl = "https://webui1.dossia.org/authserver/request_token";
	private static String authorizeUrl = "https://webui1.dossia.org/authserver/authorize";
	private static String accessUrl = "https://webui1.dossia.org/authserver/access_token";
	private static String consumerKey = "trigentWeight";
	private static String consumerSecret = "trigentSecret";
	private static String callbackUrl = "";
	
	public static final String METHOD_GET = "GET";
	public static final String METHOD_POST = "POST";
	public static final String METHOD_DELETE = "DELETE";
	
	public static final String CONTENT_TYPE_XML = "application/xml";
	public static final String CONTENT_TYPE_PDF = "application/pdf";
	public static final String CONTENT_TYPE_JPG = "image/jpeg";
	public static final String CONTENT_TYPE_GIF = "image/gif";
	public static final String CONTENT_TYPE_BMP = "image/bmp";
	public static final String CONTENT_TYPE_PNG = "image/png";
	public static final String CONTENT_TYPE_PLAIN = "text/plain";
	public static final String CONTENT_TYPE_HTML = "text/html";
	
	public static final String PARAM_API_CALL = "apiCall";
	public static final String PARAM_OPERATION = "operation";
	public static final String PARAM_METHOD = "method";
	public static final String PARAM_CONTENT_TYPE = "contentType";
	public static final String PARAM_INPUT_TEXT = "inputText";
	public static final String PARAM_OAUTH_TOKEN = "oauth_token";
	public static final String PARAM_OAUTH_TOKEN_SECRET = "oauth_token_secret";
	public static final String PARAM_OAUTH_CALLBACK = "oauth_callback";
	public static final String PARAM_RECORD_ID = "recordId";
	public static final String PARAM_KEY_ID = "keyId";
	public static final String PARAM_DOCUMENT_ID = "documentId";
	public static final String PARAM_VERSION = "version";
	public static final String PARAM_DOCUMENT_TYPE = "documentType";
	public static final String PARAM_LABEL_ID = "labelId";
	public static final String PARAM_CHILD_DOC_ID = "childDocumentId";
	
	public static final String HEADER_CONTENT_TYPE = "Content-Type";
	
	public static final String ATTRIBUTE_ACCESSOR = "accessor";
	
	public static final String OPERATION_RETURN = "accessToken";
	public static final String OPERATION_SAVE = "Save";
	public static final String OPERATION_GET = "Retrieve";
	public static final String OPERATION_DELETE = "Delete";
	
	public static final String HEADER_LOCATION = "Location";
	
	public static final String EMPTY_STRING = "";
	
	public static String VAL_RECORD_ID = "qO7k9WDv3VSDPEwSmZ3kLeakulG4y9tb";
	public static String VAL_DOCUMENT_ID = "t0FhmOk46GXiarC9VkEcmC2-CdmhhzCi";
	public static String VAL_BIN_DOCUMENT_ID = "t0FhmOk46GXiarC9VkEcmK7XuKoqk-mr";
	public static String VAL_APP_DOC_ID = "t0FhmOk46GXiarC9VkEcmJp5nDZX58N3";
	public static String VAL_LABEL_ID = "lable0000001";
	public static String VAL_DOCUMENT_TYPE = "encounter";
	
	public static String BINARY_CALL = "/binary";
	
	public static String DOC_START_DELIMETER = "<api:document id=\"";
	public static String DOC_END_DELIMETER = "\"";
	public static String DOWNLOAD_FILE_NAME = "download";
	
	
	public static final String DOSSIA_API_ENDPOINT = "https://dev-api.dossia.org/dossia-restful-api/services/v2.0";
	
	public static final String DOSSIA_API_CALL_CREATE_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/";
	public static final String DOSSIA_API_CALL_CREATE_APP_DOCUMENT_WITH_LABEL = "{dossia-api-endpoint}/records/{record-id}/apps/documents/key/{keyId}/label/{labelId}";
	public static final String DOSSIA_API_CALL_CREATE_APP_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/apps/documents/key/{keyId}";
	public static final String DOSSIA_API_CALL_RELATE_EXISTING_DOCS = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/rels/related/{childDocumentId}";
	public static final String DOSSIA_API_CALL_RELATE_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/rels/related";
	public static final String DOSSIA_API_CALL_REPLACE_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/replace";
	public static final String DOSSIA_API_CALL_REPLACE_APP_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/apps/documents/key/{keyId}/replace";
	public static final String DOSSIA_API_CALL_SUPPRESS_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/";
	public static final String DOSSIA_API_CALL_UNSUPPRESS_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/unsuppress";
	public static final String DOSSIA_API_CALL_DELETE_APP_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/apps/documents/key/{keyId}/";
	public static final String DOSSIA_API_CALL_ALL_RECORDS = "{dossia-api-endpoint}/records/";
	public static final String DOSSIA_API_CALL_RETRIEVE_RECORD = "{dossia-api-endpoint}/records/{record-id}/";
	public static final String DOSSIA_API_CALL_ROOT_LEVEL_DOCUMENTS = "{dossia-api-endpoint}/records/{record-id}/documents/";
	public static final String DOSSIA_API_CALL_SUMMARY_COUNT = "{dossia-api-endpoint}/records/{record-id}/documents/summary_count";
	public static final String DOSSIA_API_CALL_RETRIEVE_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/";
	public static final String DOSSIA_API_CALL_RETRIEVE_BIN_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/binary";
	public static final String DOSSIA_API_CALL_DOCUMENT_METADATA = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/meta/";
	public static final String DOSSIA_API_CALL_ALL_DOCUMENT_METADATA = "{dossia-api-endpoint}/records/{record-id}/documents/meta/";
	public static final String DOSSIA_API_CALL_APP_DOCS_METADATA = "{dossia-api-endpoint}/records/{record-id}/apps/documents/meta";
	public static final String DOSSIA_API_CALL_DOCUMENT_VERSIONS = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/versions";
	public static final String DOSSIA_API_CALL_SPECIFIC_VERSION = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/versions/{version-id}";
	public static final String DOSSIA_API_CALL_PARENT_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/documents/{document-id}/parent";
	public static final String DOSSIA_API_CALL_DOCUMENT_TYPE = "{dossia-api-endpoint}/records/{record-id}/documents/document_type/{document_type}/";
	public static final String DOSSIA_API_CALL_RETRIEVE_APP_DOCUMENT = "{dossia-api-endpoint}/records/{record-id}/apps/documents/key/{keyId}";
	
	public static final String REGEXP_DOSSIA_API_ENDPOINT = "\\{dossia-api-endpoint\\}";
	public static final String REGEXP_RECORD_ID = "\\{record-id\\}";
	public static final String REGEXP_DOCUMENT_ID = "\\{document-id\\}";
	public static final String REGEXP_VERSION_ID = "\\{version-id\\}";
	public static final String REGEXP_DOCUMENT_TYPE = "\\{document_type\\}";
	public static final String REGEXP_KEY_ID = "\\{keyId\\}";
	public static final String REGEXP_LABEL_ID = "\\{labelId\\}";
	public static final String REGEXP_CHILD_DOC_ID = "\\{childDocumentId\\}";


    public DossiaOAuthClient() throws IOException
    {
    	super(new HttpClient3());
    }

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to get the OAuthAccessor object.
	 ****************************************************************************************************/
    private OAuthAccessor createOAuthAccessor()
    {
        OAuthServiceProvider provider = new OAuthServiceProvider(requestUrl, authorizeUrl, accessUrl);
        OAuthConsumer consumer = new OAuthConsumer(callbackUrl, consumerKey, consumerSecret, provider);
        return new OAuthAccessor(consumer);
    }

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to get the request token.
	 ****************************************************************************************************/
    public OAuthAccessor getRequestToken() throws IOException, OAuthException, URISyntaxException
    {
    	OAuthAccessor accessor = createOAuthAccessor();
	    OAuthClient client = new OAuthClient(new HttpClient3());
        client.getRequestToken(accessor);
        return accessor;
    }

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to get the authorization url that is used for authorization of request token.
	 ****************************************************************************************************/
    public String getAuthrizationURL(HttpServletRequest request, HttpServletResponse reponse,
    		OAuthAccessor accessor)throws Exception
    {
    	String authorizationURL = accessor.consumer.serviceProvider.userAuthorizationURL;
    	URL callbackURL = new URL(new URL(request.getRequestURL().toString()),
                request.getContextPath() + request.getRequestURI().substring(request.getRequestURI().lastIndexOf("/")));
    	String targetUrl = OAuth.addParameters(authorizationURL
                , PARAM_OAUTH_TOKEN, accessor.requestToken
                , PARAM_OAUTH_CALLBACK, OAuth.addParameters(callbackURL.toString()
                , PARAM_OPERATION, OPERATION_RETURN));
    	System.out.println("TargetUrl: "+targetUrl);
        return targetUrl;
    }

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to get the document id from the output of Dossia API calls.
	 ****************************************************************************************************/
    public String getDocumentId(String queryText)throws Exception
	{
		String startDel = DOC_START_DELIMETER;
		String endDel = DOC_END_DELIMETER;
		int startIndex = queryText.indexOf(startDel)+startDel.length();
		queryText = queryText.substring(startIndex);
		int endIndex = queryText.indexOf(endDel)+endDel.length();
		return queryText.substring(0, endIndex-1);
	}

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to get the access token using request token.
	 ****************************************************************************************************/
    public void getAccessToken(OAuthAccessor accessor) throws IOException, OAuthException, URISyntaxException
    {
        List<Map.Entry> params = new ArrayList<Map.Entry>();
        params.add(new OAuth.Parameter(PARAM_OAUTH_TOKEN, accessor.requestToken));
        OAuthMessage response = invoke(accessor, METHOD_GET,  accessUrl, params);
        accessor.accessToken = response.getParameter(PARAM_OAUTH_TOKEN);
        accessor.tokenSecret = response.getParameter(PARAM_OAUTH_TOKEN_SECRET);
    }

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to make an API call with Generic http method (other than POST).
	 ****************************************************************************************************/
    public String executeAPICall(OAuthAccessor accessor, String url, String method) throws IOException, OAuthException, URISyntaxException
    {
    	OAuthMessage response = invoke(accessor, method,  url, new ArrayList<Map.Entry>());
        String contentType = response.getHeader(HEADER_CONTENT_TYPE);
        if(contentType.equals(DossiaOAuthClient.CONTENT_TYPE_XML) 
        		|| contentType.contains(DossiaOAuthClient.CONTENT_TYPE_PLAIN)
        		|| contentType.contains(DossiaOAuthClient.CONTENT_TYPE_HTML))
        {
            return response.readBodyAsString();
        }
        else
        {
        	InputStream is = response.getBodyAsStream();
        	File file = new File(DOWNLOAD_FILE_NAME + System.currentTimeMillis() + "." + contentType.substring(contentType.indexOf("/")+1));
            System.out.println(file.getAbsolutePath());
            FileOutputStream output = new FileOutputStream(file);

    		try
    		{
    			byte buf[]=new byte[1024];
    		    int len;
    		    while((len=is.read(buf))>0)
    		    	output.write(buf,0,len);
    		}
    		finally
    		{
    			output.close();
    		}
    		return "Downloaded to " + file.getAbsolutePath();
        }
    }

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to make an API call with POST http method.
	 ****************************************************************************************************/
    public String executeAPICall(OAuthAccessor accessor, String httpMethod,
			String url, Collection<? extends Map.Entry> parameters, String contentType, String content)
	throws IOException, OAuthException, URISyntaxException {
    	return executeAPICall(accessor, httpMethod, url, parameters, contentType, new ByteArrayInputStream(content.getBytes()));
    }

    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to make an API call with POST http method.
	 ****************************************************************************************************/
    public String executeAPICall(OAuthAccessor accessor, String httpMethod,
			String url, Collection<? extends Map.Entry> parameters, String contentType, InputStream is)
	throws IOException, OAuthException, URISyntaxException {
    	OAuthMessage message = null;
    	try
    	{
    		OAuthMessage request = new OAuthMessage(httpMethod, url, parameters,is);
    		request.getHeaders().add(new OAuth.Parameter(HttpMessage.CONTENT_TYPE,contentType));
    		Object accepted = accessor.consumer.getProperty(OAuthConsumer.ACCEPT_ENCODING);
    		if (accepted != null) {
    			request.getHeaders().add(new OAuth.Parameter(HttpMessage.ACCEPT_ENCODING, accepted.toString()));
    		}
    		request.addRequiredParameters(accessor);
    		Object ps = accessor.consumer.getProperty(PARAMETER_STYLE);
    		ParameterStyle style = (ps == null) ? ParameterStyle.BODY : Enum.valueOf(ParameterStyle.class, ps.toString());
    		message = invoke(request, style);
    	}
    	catch(Exception e){e.printStackTrace();}
    	return message.readBodyAsString();
	}
    
    /****************************************************************************************************
	 * @Author: Dossia
	 * This method is to replace one string with other string.
	 ****************************************************************************************************/
    public String replace(String regex, String value, String call)
	{
		Pattern pattern = Pattern.compile(regex);
        Matcher matcher = pattern.matcher(call);
        return matcher.replaceAll(value);
	}
}
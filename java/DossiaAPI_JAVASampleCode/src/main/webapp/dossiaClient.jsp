   <%--
   
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
    
   
   --%>
   
<%@page import="java.util.List, java.util.Iterator, java.io.InputStream"%>
<%@page import="org.dossia.oauth.DossiaOAuthClient"%>
<%@page import="net.oauth.OAuth, net.oauth.OAuthMessage, net.oauth.OAuthAccessor, net.oauth.server.HttpRequestMessage"%>
<%@ page import="org.apache.commons.fileupload.*,org.apache.commons.fileupload.servlet.ServletFileUpload,org.apache.commons.fileupload.disk.DiskFileItemFactory,org.apache.commons.io.FilenameUtils"%>

<%
	//Dossia API call.
	String apiCall = request.getParameter(DossiaOAuthClient.PARAM_API_CALL) == null ? DossiaOAuthClient.EMPTY_STRING : request.getParameter(DossiaOAuthClient.PARAM_API_CALL);
	String resp = DossiaOAuthClient.EMPTY_STRING;
	String recordId = DossiaOAuthClient.EMPTY_STRING;
	String keyId = DossiaOAuthClient.EMPTY_STRING;
	String documentId = DossiaOAuthClient.EMPTY_STRING;
	String version = DossiaOAuthClient.EMPTY_STRING;
	String documentType = DossiaOAuthClient.EMPTY_STRING;
	String labelId = DossiaOAuthClient.EMPTY_STRING;
	String childDocumentId = DossiaOAuthClient.EMPTY_STRING;
	
	try
	{
		System.out.println("Before");
		String operation = DossiaOAuthClient.EMPTY_STRING, method = DossiaOAuthClient.EMPTY_STRING;
		String contentType = null, inputText = DossiaOAuthClient.EMPTY_STRING;
		InputStream is = null;
		if (ServletFileUpload.isMultipartContent(request))
		{
			ServletFileUpload servletFileUpload = new ServletFileUpload(new DiskFileItemFactory());
			List fileItemsList = servletFileUpload.parseRequest(request);
			Iterator it = fileItemsList.iterator();
			
			while (it.hasNext())
			{
				FileItem fileItem = (FileItem)it.next();
				if (fileItem.isFormField())
				{
					if ((operation == null || DossiaOAuthClient.EMPTY_STRING.equals(operation)) && fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_OPERATION))
						operation = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_METHOD))
						method = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_API_CALL))
						apiCall = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_INPUT_TEXT))
						inputText = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_RECORD_ID))
						recordId = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_KEY_ID))
						keyId = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_DOCUMENT_ID))
						documentId = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_VERSION))
						version = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_DOCUMENT_TYPE))
						documentType = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_LABEL_ID))
						labelId = fileItem.getString();
					if (fileItem.getFieldName().equals(DossiaOAuthClient.PARAM_CHILD_DOC_ID))
						childDocumentId = fileItem.getString();
				}
				else if (fileItem!=null)
				{
					is = fileItem.getInputStream();
					contentType = fileItem.getContentType();
				}
			}
		}
		else
		{
			operation = request.getParameter(DossiaOAuthClient.PARAM_OPERATION);
			method = request.getParameter(DossiaOAuthClient.PARAM_METHOD);
			apiCall = request.getParameter(DossiaOAuthClient.PARAM_API_CALL);
			inputText = request.getParameter(DossiaOAuthClient.PARAM_INPUT_TEXT);
			recordId = request.getParameter(DossiaOAuthClient.PARAM_RECORD_ID);
			keyId = request.getParameter(DossiaOAuthClient.PARAM_KEY_ID);
			documentId = request.getParameter(DossiaOAuthClient.PARAM_DOCUMENT_ID);
			version = request.getParameter(DossiaOAuthClient.PARAM_VERSION);
			documentType = request.getParameter(DossiaOAuthClient.PARAM_DOCUMENT_TYPE);
			labelId = request.getParameter(DossiaOAuthClient.PARAM_LABEL_ID);
			childDocumentId = request.getParameter(DossiaOAuthClient.PARAM_CHILD_DOC_ID);
			
			if(method == null)
			{
				apiCall = (String)request.getAttribute(DossiaOAuthClient.PARAM_API_CALL);
				method = (String)request.getAttribute(DossiaOAuthClient.PARAM_METHOD);
			}
		}
		System.out.println("Before assinging blank.");
		if(operation == null) operation = DossiaOAuthClient.EMPTY_STRING;
		if(contentType == null) contentType = DossiaOAuthClient.CONTENT_TYPE_XML;
		if(inputText == null) inputText = DossiaOAuthClient.EMPTY_STRING;
		if(apiCall == null) apiCall = DossiaOAuthClient.EMPTY_STRING;
		if(method == null) method = DossiaOAuthClient.EMPTY_STRING;
		
		if(recordId == null) recordId = DossiaOAuthClient.EMPTY_STRING;
		if(keyId == null) keyId = DossiaOAuthClient.EMPTY_STRING;
		if(documentId == null) documentId = DossiaOAuthClient.EMPTY_STRING;
		if(version == null) version = DossiaOAuthClient.EMPTY_STRING;
		if(documentType == null) documentType = DossiaOAuthClient.EMPTY_STRING;
		if(labelId == null) labelId = DossiaOAuthClient.EMPTY_STRING;
		if(childDocumentId == null) childDocumentId = DossiaOAuthClient.EMPTY_STRING;
		
		DossiaOAuthClient dossiaOAuthClient = new DossiaOAuthClient();
		
		apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_DOSSIA_API_ENDPOINT, DossiaOAuthClient.DOSSIA_API_ENDPOINT, apiCall);
		
		System.out.println("Before Replace");
		if(!recordId.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_RECORD_ID, recordId, apiCall);
		}
		if(!keyId.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_KEY_ID, keyId, apiCall);
		}
		if(!documentId.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_DOCUMENT_ID, documentId, apiCall);
		}
		if(!version.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_VERSION_ID, version, apiCall);
		}
		if(!documentType.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_DOCUMENT_TYPE, documentType, apiCall);
		}
		if(!labelId.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_LABEL_ID, labelId, apiCall);
		}
		if(!childDocumentId.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			apiCall = dossiaOAuthClient.replace(DossiaOAuthClient.REGEXP_CHILD_DOC_ID, childDocumentId, apiCall);
		}
		System.out.println("After Replace");
		System.out.println("Operation: " + operation);
		
		OAuthAccessor accessor = null;
		if(operation.trim().equals(DossiaOAuthClient.EMPTY_STRING)){
			//Getting Request Token.
			accessor = dossiaOAuthClient.getRequestToken();
			session.setAttribute(DossiaOAuthClient.ATTRIBUTE_ACCESSOR,accessor);
			//Getting authorization URL.
			String targetURL = dossiaOAuthClient.getAuthrizationURL(request, response, accessor);
			
			if (targetURL != null){
				//Redirecting for authorization.
               // response.setStatus(HttpServletResponse.SC_MOVED_TEMPORARILY);
               // response.setHeader("Location", targetURL);
				response.sendRedirect (targetURL);
            }
		}
		else if(operation.equalsIgnoreCase(DossiaOAuthClient.OPERATION_RETURN)){
			//Getting Access Token.
			accessor = (OAuthAccessor)session.getAttribute(DossiaOAuthClient.ATTRIBUTE_ACCESSOR);
			dossiaOAuthClient.getAccessToken(accessor);
			
			if(method.equalsIgnoreCase(DossiaOAuthClient.METHOD_GET) || method.equalsIgnoreCase(DossiaOAuthClient.METHOD_DELETE)){
				System.out.println("########Operation: " + DossiaOAuthClient.METHOD_GET);
				resp = dossiaOAuthClient.executeAPICall(accessor, apiCall, method);
			}
		}
		else if(operation.equalsIgnoreCase(DossiaOAuthClient.OPERATION_GET) || operation.equalsIgnoreCase(DossiaOAuthClient.OPERATION_DELETE)){
			
			accessor = (OAuthAccessor)session.getAttribute(DossiaOAuthClient.ATTRIBUTE_ACCESSOR);
			System.out.println("*********Operation: " + DossiaOAuthClient.METHOD_GET);
			System.out.println("*********method: " + method);
			System.out.println("*********apiCall: " + apiCall);
			System.out.println("*********accessor: " + accessor);
			
			resp = dossiaOAuthClient.executeAPICall(accessor, apiCall, method);
		}
		else if(operation.equalsIgnoreCase(DossiaOAuthClient.OPERATION_SAVE)){
			if(method.equals(DossiaOAuthClient.METHOD_POST) && (contentType.equalsIgnoreCase(DossiaOAuthClient.CONTENT_TYPE_XML) || !DossiaOAuthClient.EMPTY_STRING.equals(inputText)))
			{
				accessor = (OAuthAccessor)session.getAttribute(DossiaOAuthClient.ATTRIBUTE_ACCESSOR);
				List<OAuth.Parameter> parameters = HttpRequestMessage.getParameters(request);
				
				//Making the Dossia API call.
				resp = dossiaOAuthClient.executeAPICall(accessor, method, apiCall, parameters, DossiaOAuthClient.CONTENT_TYPE_XML, inputText);
				if(!contentType.equalsIgnoreCase(DossiaOAuthClient.CONTENT_TYPE_XML))
				{
					String docId = dossiaOAuthClient.getDocumentId(resp);
					apiCall = apiCall + docId + DossiaOAuthClient.BINARY_CALL;
				}
			}
			if(method.equals(DossiaOAuthClient.METHOD_POST) && !contentType.equalsIgnoreCase(DossiaOAuthClient.CONTENT_TYPE_XML))
			{
				accessor = (OAuthAccessor)session.getAttribute(DossiaOAuthClient.ATTRIBUTE_ACCESSOR);
				List<OAuth.Parameter> parameters = HttpRequestMessage.getParameters(request);
				//Making the Dossia API call.
				resp = dossiaOAuthClient.executeAPICall(accessor, method, apiCall, parameters, contentType, is);
			}
		}
	}
	catch(Exception e){	System.out.println("Exception: " + e); e.printStackTrace(); }
%>
	<br><b>API Call: </b><%=apiCall%><br>
	<b>Output Text:</b><br><textarea cols="50" rows="20" name="outputText"><%=resp%></textarea>
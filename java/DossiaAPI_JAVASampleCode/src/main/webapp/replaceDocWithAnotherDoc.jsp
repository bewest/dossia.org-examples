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

<%@page import="org.dossia.oauth.DossiaOAuthClient"%>
<jsp:include page="/banner.jsp" />
<html><head><title>Replacing a Document with Another Document</title></head><body>
	
	<form method="POST" action="./replaceDocWithAnotherDoc.jsp">
		<input type="hidden" name="<%=DossiaOAuthClient.PARAM_API_CALL%>" value="<%=DossiaOAuthClient.DOSSIA_API_CALL_REPLACE_DOCUMENT%>"/>
		<input type="hidden" name="<%=DossiaOAuthClient.PARAM_OPERATION%>" value="<%=DossiaOAuthClient.OPERATION_SAVE%>">
		<input type="hidden" name="<%=DossiaOAuthClient.PARAM_METHOD%>" value="<%=DossiaOAuthClient.METHOD_POST%>"/>
		<br>
		<br>Record ID: <input type="text" name="recordId"/>
		<br>Document ID: <input type="text" name="documentId"/>
		<br>
		<br>Input Text:<br><textarea cols="50" rows="20" name="inputText"></textarea>
		<br><input type="submit" name="submit" value="Save"/>
	</form>
	<%@include file="dossiaClient.jsp" %>
</body></html>
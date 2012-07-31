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
<html><head><title>Retrieving a Collection of Documents by Type</title></head><body>
	
	<form method="POST" action="./retrievingCollectionOfDocsByType.jsp" enctype="multipart/form-data">
		<input type="hidden" name="<%=DossiaOAuthClient.PARAM_API_CALL%>" value="<%=DossiaOAuthClient.DOSSIA_API_CALL_DOCUMENT_TYPE%>"/>
		<input type="hidden" name="<%=DossiaOAuthClient.PARAM_OPERATION%>" value="<%=DossiaOAuthClient.OPERATION_GET%>">
		<input type="hidden" name="<%=DossiaOAuthClient.PARAM_METHOD%>" value="<%=DossiaOAuthClient.METHOD_GET%>"/>
		<br>Record ID: <input type="text" name="recordId"/>
		<br>Document Type: 
		<select name="documentType">
			<option value="Allergy">Allergy</option>
			<option value="Annotation">Annotation</option>
			<option value="Appointment">Appointment</option>
			<option value="Association">Association</option>
			<option value="BinaryData">BinaryData</option>
			<option value="Dental">Dental</option>
			<option value="Encounter">Encounter</option>
			<option value="Equipment">Equipment</option>
			<option value="FamilyHistory">FamilyHistory</option>
			<option value="Immunization">Immunization</option>
			<option value="Insurance">Insurance</option>
			<option value="LabTest">LabTest</option>
			<option value="Measurement">Measurement</option>
			<option value="Medication">Medication</option>
			<option value="Micro">Micro</option>
			<option value="Problem">Problem</option>
			<option value="Procedure">Procedure</option>
		</select>
		
		<br><input type="submit" name="submit" value="Get"/>
	</form>
	
	<%@include file="dossiaClient.jsp" %>
</body></html>
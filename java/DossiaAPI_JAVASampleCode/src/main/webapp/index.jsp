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

<html>
<head><title>Dossia API Calls</title></head>
<body>
<jsp:include page="/banner.jsp" />
<style>
#uriLinks a {
	clear: both;
	float: left;
} 
</style>
Dossia API Calls<br>
<div id="uriLinks">
<a href="createNewDocument.jsp">1. Create New Document</a>
<a href="createBinDocument.jsp">2. Create Binary Document</a>
<a href="createAppDocumentWithLabel.jsp">3. Create Application Document with label</a>
<a href="createAppDocument.jsp">4. Create Application Document</a>
<a href="relateExistingDocsAsParentAndChild.jsp">5. Relating existing documents as parent and child documents</a>
<a href="relateDocToParentDoc.jsp">6. Relating a Document to a Parent Document</a>
<a href="replaceDocWithAnotherDoc.jsp">7. Replacing a Document with Another Document</a>
<a href="replaceAppDocument.jsp">8. Replacing an Application document with another document</a>
<a href="suppressDocument.jsp">9. Document Suppression</a>
<a href="unsuppressDocument.jsp">10. UnSuppress a document</a>
<a href="deleteAppDocument.jsp">11. Application Document Deletion</a>
<a href="getRecords.jsp">12. Retrieving available Records</a>
<a href="retrieveRecord.jsp">13. Retrieving a Record</a>
<a href="retrievingRootLevelDocInRecord.jsp">14.Retrieving Root Level Documents Within a Record</a>
<a href="retrievingDocSummaryCounts.jsp">15.Retrieving Document Summary Counts</a>
<a href="retrievingDocFromDocID.jsp">16.Retrieving a Document From a Document ID</a>
<a href="retrievingBinDoc.jsp">17.Retrieving a Binary Document</a>
<a href="retrieveMetadata.jsp">18.Metadata</a>
<a href="retrieveRootLevelMetadata.jsp">19.Metadata for all root level documents</a>
<a href="retrieveAppDocMetadata.jsp">20.Metadata for Application documents</a>
<a href="versions.jsp">21.Versions</a>
<a href="specificDocVersion.jsp">22.Specific Document Version</a>
<a href="retrievingParentDoc.jsp">23.Retrieving parent document</a>
<a href="retrievingCollectionOfDocsByType.jsp">24.Retrieving a Collection of Documents by Type</a>
<a href="retrieveAppDocByKeyId.jsp">25.Retrieving a Application document by Key id</a>
</div>
</body>
</html>

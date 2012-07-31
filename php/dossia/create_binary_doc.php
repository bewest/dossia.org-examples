<?php

/**
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
 
//Initialize dossia client
require_once 'lib/Dossia_Client.php';
$dossia = new Dossia_Client();

/*Get the tokens from Dossia OAuth server and initialize the access token and token secret.*/
$dossia->get_tokens();

if(isset($_REQUEST['record_id']) && !empty($_REQUEST['record_id'])){

	/*Initialize the record ID for document creation */
	$dossia->record_id = trim($_REQUEST['record_id']);

	//Check file upload
	if(isset($_FILES['userfile'])) {
		if ($_FILES['userfile']['error'] > 0) {
			switch ($_FILES['userfile']['error']) {
				case 1:
					echo 'The uploaded file exceeds the upload_max_filesize directive in php.ini';
					break;
				case 2:
					echo 'The uploaded file exceeds the MAX_FILE_SIZE directive that was specified in the HTML form';
					break;
				case 3:
					echo 'The uploaded file was only partially uploaded';
					break;
				case 4:
					echo 'No file was uploaded';
					break;
				case 5:
					echo 'Missing a temporary folder';
					break;
				case 6:
					echo 'Failed to write file to disk';
					break;
				case 7:
					echo 'File upload stopped by extension';
					break;
				default:
					echo 'Unknown upload error';
					break;
			}
			return;
		}

		//check content type
		switch ($_FILES['userfile']['type']){
			case 'application/pdf':
				break;
			case 'image/jpeg':
				break;
			case 'image/bmp':
				break;
			case 'image/gif':
				break;
			default:
				echo 'Invalid file type: Allowed file types are pdf,jpeg,bmp and gif';
				return;
		}
		$uploaddir = dirname(__FILE__).'/data/';
		$uploadfile = $uploaddir . basename($_FILES['userfile']['name']);
		if (!move_uploaded_file($_FILES['userfile']['tmp_name'], $uploadfile)) {
		   echo "<br/>Possible file upload attack!\n";
		}
		$file_path = $uploaddir.$_FILES['userfile']['name'];
		$file_type = $_FILES['userfile']['type'];
	
		$input_xml = trim($_POST["api_input_data"]);
		$input_xml = str_replace('\"','"',$input_xml);
		
		if(empty($input_xml)){
		$input_xml = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
						<api:container xmlns:api="http://www.dossia.org/v2.0/api" xmlns:phr="http://www.dossia.org/v2.0/xml/phr" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.dossia.org/v2.0/api http://www.dossia.org/v2.0/api/container.xsd">
						    <api:document>
						        <api:payload>
						            <phr:BinaryData ProfessionallySourced="true">
						                <phr:Origin Classification="PHR">
						                    <phr:Organization ProviderType="Other">
						                        <phr:OrganizationName>Dossia</phr:OrganizationName>
						                        <phr:Address>
						                            <phr:StreetAddress1>One Cambridge Center</phr:StreetAddress1>
						                            <phr:StreetAddress2>Suite 11</phr:StreetAddress2>
						                            <phr:County>Cambridge</phr:County>
						                            <phr:City>Boston</phr:City>
						                            <phr:State>CA</phr:State>
						                            <phr:PostalCode>5231-02142</phr:PostalCode>
						                            <phr:Country>USA</phr:Country>
						                        </phr:Address>
						                        <phr:Contact type="Work Phone">900-868-8566</phr:Contact>
						                        <phr:ProviderIdentifier Type="PHA">
						                            <phr:Identifier>1999-12</phr:Identifier>
						                            <phr:Description>Dossia provides PHR services.</phr:Description>
						                        </phr:ProviderIdentifier>
						                    </phr:Organization>
						                </phr:Origin>
						                <phr:Date Type="Actual">
						                    <phr:StartDate>'.date('Y-m-d').'</phr:StartDate>
						                </phr:Date>
						                <phr:Filename>'.$_FILES['userfile']['name'].'</phr:Filename>
						                <phr:MimeType>'.$_FILES['userfile']['type'].'</phr:MimeType>
						                <phr:Length>'.$_FILES['userfile']['size'].'</phr:Length>
						            </phr:BinaryData>
						        </api:payload>
						    </api:document>
						</api:container>';
		}	
		
		$api_service = "records/" . $dossia->record_id . "/documents/";
		$test_token = new OAuthConsumer($dossia->token, $dossia->token_secret);
		$data_req = OAuthRequest::from_consumer_and_token($dossia->the_consumer, $test_token, "POST", $dossia->service_path . $api_service, array());
		$data_req->sign_request($dossia->sig_method, $dossia->the_consumer, $test_token);

		/* Initialize the CURL Request */
		$ch = curl_init();
		curl_setopt($ch, CURLOPT_URL, $data_req);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
		curl_setopt($ch, CURLOPT_HTTPHEADER, array('Content-Type: application/xml', "Authorization: OAuth " . implode(",", $data_req->get_parameters())));
		curl_setopt($ch, CURLOPT_POST, true);
		curl_setopt($ch, CURLOPT_POSTFIELDS, $input_xml);
		curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 60);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
		curl_setopt($ch, CURLOPT_FOLLOWLOCATION, 1);
		$output  = curl_exec($ch);		
		curl_close($ch);

		/*Step 2 Send binary conent through http request body by triggering the /binary call on that document id generated for Step 1 */

		/*Get document ID from the output of the above call*/

		preg_match('/<api:document id="(.+)" /', $output, $matches);
		$doc_id = array_pop($matches);

		/*Intialize the API call*/
		$api_service = 'records/' . $dossia->record_id . '/documents/'.$doc_id.'/binary';
		$test_token = new OAuthConsumer($dossia->token, $dossia->token_secret);
		$data_req = OAuthRequest::from_consumer_and_token($dossia->the_consumer, $test_token, "POST", $dossia->service_path . $api_service, array());
		$data_req->sign_request($dossia->sig_method, $dossia->the_consumer, $test_token);
		$params = file_get_contents($file_path);
		$ch = curl_init();            
		curl_setopt($ch, CURLOPT_URL, $data_req);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
		curl_setopt($ch, CURLOPT_HTTPHEADER, array("Content-Type: $file_type", "Content-Length:" . strlen($params), "Authorization: OAuth " . implode(",", $data_req->get_parameters())));
		curl_setopt($ch, CURLOPT_POST, true);
		curl_setopt($ch, CURLOPT_POSTFIELDS, $params);
		curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 180);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
		curl_setopt($ch, CURLOPT_FOLLOWLOCATION, 1);
		$api_output_data  = curl_exec($ch);
		$info   = curl_getinfo($ch);
		if($info['http_code'] >= 400) $api_output_data = "Status code: ".$info["http_code"];		
		curl_close($ch);
	}
}
?>

<!-- HTML page for Demo -->
<html>
    <head>
        <title>
           Dossia Application Programming Interface - PHP Version
        </title>
    </head>
    <body>
       <table width="100%" cellpadding="10" cellspacing="0" border="0">
        <tr>
            <td bgcolor="#00F0F0" width="15%"><img src="dossia.png"></td>
            <td bgcolor="#00F0F0"><h2>Dossia Application Programming Interface - PHP Version</h2>
            New Binary Document
            </td>
        </tr>
        <tr><td colspan="2" align="right"><a href="index.php">Return to Home</a></td></tr>
       </table>
       <table width="500" cellpadding="0" align="center" cellspacing="0" border="0">
        <tr>
            <td bgcolor="#FDB98C" >
                <?php
                    if(!empty($dossia->error_msg)) {
                        echo "Error Details : ";
                        echo $dossia->get_error();
                    }
                ?>
            </td>
        </tr>
       </table>
        <br/>
       <form enctype="multipart/form-data" name="getDossiaData" action="<?php echo $_SERVER['PHP_SELF'];?>" method="POST">
            <input type="hidden" name="token" value="<?php echo $dossia->token;?>">
            <input type="hidden" name="token_secret" value="<?php echo $dossia->token_secret;?>">
            <table width="100%" border="0" cellspacing="6" cellpadding="0">
			   <tr>
					   <td width="30%" align="right">
								Record ID:
						</td>
						<td width="70%">
								<input type="text" name="record_id" value="" size="50"/>
						  </td>
				</tr>
                <tr>
                   <td width="30%" align="right">
                            Upload File:
                    </td>
                    <td width="70%">
                            <input type="hidden" name="MAX_FILE_SIZE" value="10485760" />
                            <input name="userfile" type="file" size="50"/>                            
                      </td>
                </tr>
                <tr>
                    <td width="30%" align="right" valign="top">
                            API Input:
                    </td>
                    <td width="70%">
                        <textarea name="api_input_data" cols="70" rows="12"><?php echo $input_xml;?></textarea><br>
                    </td>
                </tr>
                <tr>
                    <td width="30%" align="right" valign="top">
                            &nbsp;
                    </td>
                    <td width="70%">
                        <input type="submit" name="call_api" value="Create">
                    </td>
                </tr>
                <tr>
                    <td align="right" valign="top">
                            API Output:
                    </td>
                    <td>
                        <textarea cols="70" rows="12"><?php echo $output;?></textarea><br>
                    </td>
                </tr>
                <tr>
                    <td align="right" valign="top">
                            API Output for Binary content:
                    </td>
                    <td>
                        <textarea cols="70" rows="12"><?php echo $api_output_data;?></textarea><br>
                    </td>
                </tr>
            </table>
        </form>
    </body>
</html>
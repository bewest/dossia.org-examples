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

		$upload_dir = dirname(__FILE__).'/data/';
		$upload_file = $upload_dir . basename($_FILES['userfile']['name']);
		if (!move_uploaded_file($_FILES['userfile']['tmp_name'], $upload_file)) {
		   echo "<br/>Possible file upload attack!\n";
		   return;
		}
		$file_path = $upload_dir.$_FILES['userfile']['name'];
		$file_type = $_FILES['userfile']['type'];

		if(isset($_REQUEST['key']) && !empty($_REQUEST['key'])){
			$key = $_REQUEST['key'];
		}else{
			die("Invalid key provided..");
		}

		/*Initialize API call */
		$api_service = "records/" . $dossia->record_id . "/apps/documents/key/".$key; 
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
            <td bgcolor="#00F0F0"><h2>Dossia Application Programming Interface - PHP Version</h2> New Application Document </td>
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
                            Enter Key:
                    </td>
                    <td width="70%">
                            <input type="text" name="key" value=""/>
                      </td>
                </tr>
                <tr>
                   <td width="30%" align="right">
                            Upload File:
                    </td>
                    <td width="70%">
                            <input type="hidden" name="MAX_FILE_SIZE" value="300000" />
                            <input name="userfile" type="file"size="50" />
                      </td>
                </tr>
                <tr>
                   <td width="30%" align="right">
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
                        <textarea cols="70" rows="12"><?php echo $api_output_data;?></textarea><br>
                    </td>
                </tr>
            </table>
        </form>
    </body>
</html>
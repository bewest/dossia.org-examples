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

	/*Initialize the input data*/
	$parent_doc_id 	 = $_REQUEST['parent_doc_id'];
	$input_data = trim($_POST["api_input_data"]);
	$input_data = str_replace('\"','"',$input_data);
	
	/*Initialize API call */
	$api_service = "records/" . $dossia->record_id . "/documents/" . $parent_doc_id . "/rels/related";
	$test_token = new OAuthConsumer($dossia->token, $dossia->token_secret);
	$data_req = OAuthRequest::from_consumer_and_token($dossia->the_consumer, $test_token, "POST", $dossia->service_path . $api_service, array());
	$data_req->sign_request($dossia->sig_method, $dossia->the_consumer, $test_token);
	$ch = curl_init();
	curl_setopt($ch, CURLOPT_URL,  $data_req);
	curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
	curl_setopt($ch, CURLOPT_HTTPHEADER, array('Content-Type: application/xml', "Authorization: OAuth " . implode(",", $data_req->get_parameters())));
	curl_setopt($ch, CURLOPT_POST, true);
	curl_setopt($ch, CURLOPT_POSTFIELDS, $input_data);
	curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 60);
	curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
	curl_setopt($ch, CURLOPT_FOLLOWLOCATION, 1);
	$api_output_data = curl_exec($ch);
	$info   = curl_getinfo($ch);
	if($info['http_code'] >= 400) $api_output_data = "Status code: ".$info["http_code"];
	
	curl_close($ch);
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
			Relating a Document to a Parent Document </td>
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
        <form name="getDossiaData" action="<?php echo $_SERVER['PHP_SELF'];?>" method="post">
            <input type="hidden" name="token" value="<?php echo $dossia->token;?>">
            <input type="hidden" name="token_secret" value="<?php echo $dossia->token_secret;?>">
            <table width="100%" border="0" cellspacing="6" cellpadding="0">
			 <tr>
					   <td width="30%" align="right">
								Record ID:
						</td>
						<td width="70%">
								<input type="text" name="record_id" value="<?=$_REQUEST['record_id']?>" size="50"/>
						  </td>
					</tr>
				 <tr>
					   <td width="30%" align="right">
								Parent document ID:
						</td>
						<td width="70%">
								<input type="text" name="parent_doc_id" value="<?=$_REQUEST['parent_doc_id']?>" size="50"/>								 
						  </td>
				</tr>
				<tr>
                    <td align="right" valign="top">
                            API Input:                            
                    </td>
                    <td>
                        <textarea name="api_input_data" cols="70" rows="12"><?php echo $input_data;?></textarea><br>
                    </td>
                </tr>
				<tr>
					<td>&nbsp;</td><td><input type="submit" name="call_api" value="Submit"></td>
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
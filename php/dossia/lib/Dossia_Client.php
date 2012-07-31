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
 
/**
* Dissia API client library
*
* @package Dossia API PHP Client 
* @author Trigent
* @version 1.0
*/

/**
 * OAuth , third party Authentication library
 */
require_once('config.php');
require_once('OAuth.php');
require_once('OAuth_TestServer.php');

class Dossia_Client
{
    public $domain;
    public $base;
    public $base_url;
    public $proxy;
    public $customer_key;
    public $customer_secret;
    public $token;
    public $token_secret;
    public $user_sig_method;
    public $service_path;
    public $record_id;
    public $request_token_url;
    public $authorize_token_url;
    public $access_token_url;
    public $the_consumer;
    public $error_msg;
	public $sig_methods;
	public $sig_method;

    /**
     * The class constructor
     *
     * @access public
     */
    public function __construct()
    {
		/**********************************************************/
		/* Change the following based on your environment		  */
		/**********************************************************/
		
		$this->domain		= USER_DOMAIN;
		$this->base_dir		= USER_BASE_DIR;
		$this->http_method	= USER_HTTP_METHOD;
		$this->user_sig_method = USER_SIG_METHOD; 
		$this->proxy = USER_PROXY;

		/*Initialize with the Dossia OAuth URL end-points*/
		$this->service_path		= SERVICE_PATH;
		$this->request_token_url	= REQUEST_TOKEN_URL;
		$this->authorize_token_url= AUTHORIZE_TOKEN_URL;
		$this->access_token_url	= ACCESS_TOKEN_URL;

		/*Initialize the key and secret with Dossia supplied key and Dossia generated secret respectively.*/
		$this->customer_key	 = CUSTOMER_KEY;
		$this->customer_secret = CUSTOMER_SECRET;

		/* Dont Change anything after this */
        $this->base_url = $this->http_method.'://'.$this->domain.'/'.$this->base_dir;        
        $this->test_server  = new TestOAuthServer(new MockOAuthDataStore());
        $this->hmac_method  = new OAuthSignatureMethod_HMAC_SHA1();
        $this->plaintext_method	= new OAuthSignatureMethod_PLAINTEXT();
        $this->rsa_method   = new TestOAuthSignatureMethod_RSA_SHA1();
        $this->test_server->add_signature_method($this->hmac_method);
        $this->test_server->add_signature_method($this->plaintext_method);
        $this->test_server->add_signature_method($this->rsa_method);
        $this->sig_methods = $this->test_server->get_signature_methods();
		$this->sig_method  = $this->hmac_method;
		if ($this->user_sig_method) $this->sig_method = $this->sig_methods[$this->user_sig_method];
    }

    /**
     * Get access tokens from Dissia server
     *
     * @access public
     * @param NIL
     * @return Array (token,secret)
     */    
    public function get_tokens()
    {
        $return_page = basename($_SERVER["PHP_SELF"]);
        if(isset($_REQUEST['token'])) $this->token = $_REQUEST['token'];
        if(isset($_REQUEST['token_secret'])) $this->token_secret = $_REQUEST['token_secret'];
        $this->the_consumer = new OAuthConsumer($this->customer_key, $this->customer_secret, NULL);
        $test_token = NULL;
        if((!isset($_GET) || empty($_GET)) && (!isset($_POST) || empty($_POST))) {
            $req_url = OAuthRequest::from_consumer_and_token($this->the_consumer, NULL, "GET", $this->request_token_url, array());
            $req_url->sign_request($this->sig_method, $this->the_consumer, NULL);
            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL, $req_url);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            curl_setopt($ch, CURLOPT_HTTPHEADER, array("Authorization: OAuth " . implode(",", $req_url->get_parameters())));
            curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 60);
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
            curl_setopt($ch, CURLOPT_FOLLOWLOCATION, 1);
            if(!empty($this->proxy)) curl_setopt($ch, CURLOPT_PROXY, $this->proxy);
            $output = curl_exec($ch);
            $info   = curl_getinfo($ch);
            $curlerr= curl_error($ch);
            curl_close($ch);
            if(!empty($curlerr)) {
                $this->error_msg[] = $curlerr;
            }
            elseif(!isset($info["http_code"]) || $info["http_code"] != 200) {
                $this->error_msg[] = $output;
            }
            if(empty($curlerr)) {
                $token_arr = array();
                if(preg_match_all("/oauth_token=(.*)&oauth_token_secret=(.*)/ims", $output, $token_arr)) {
                    if(isset($token_arr[1][0])) {
                        $this->token = $token_arr[1][0];
                    }
                    if(isset($token_arr[2][0])) {
                        $this->token_secret = $token_arr[2][0];
                    }
                    if(!empty($this->token) && !empty($this->token_secret)) {
                        $callback_url = $this->base_url . '/' . $return_page . '?token='.$this->token.'&token_secret='.$this->token_secret.'&endpoint='.urlencode($this->authorize_token_url);
                        $auth_url = $this->authorize_token_url . '?oauth_token='.$this->token.'&oauth_callback='.urlencode($callback_url);
                        header("Location: $auth_url");
                        exit;
                    }
                }
            }
            if(empty($this->token) && empty($this->token_secret)) {
                $this->error_msg[] = "Invalid request";
            }
        }
        elseif (isset($_GET) && !empty($_GET)) {
            if ($this->token) {
                $test_token = new OAuthConsumer($this->token, $this->token_secret);
                $acc_req = OAuthRequest::from_consumer_and_token($this->the_consumer, $test_token, "GET", $this->access_token_url, array());
                $acc_req->sign_request($this->sig_method, $this->the_consumer, $test_token);
                $ch = curl_init();
                $url = $acc_req;
                curl_setopt($ch, CURLOPT_URL, $url);
                curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
                curl_setopt($ch, CURLOPT_HTTPHEADER, array("Authorization: OAuth " . implode(",", $acc_req->get_parameters())));
                curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 60);
                curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
                curl_setopt($ch, CURLOPT_FOLLOWLOCATION, 1);
                if(!empty($this->proxy)) curl_setopt($ch, CURLOPT_PROXY, $this->proxy);
                $output = curl_exec($ch);
                $info = curl_getinfo($ch);
                $curlerr = curl_error($ch);
                curl_close($ch);
                if(!empty($curlerr)) {
                    $this->error_msg[] = $curlerr;
                }
                elseif(!isset($info["http_code"]) || $info["http_code"] != 200) {
                    $this->error_msg[] = $output;
                }
                if(empty($curlerr)) {
                    $token_arr = array();
                    if(preg_match_all("/oauth_token=(.*)&oauth_token_secret=(.*)/ims", $output, $token_arr)) {
                        if(isset($token_arr[1][0])) {
                            $this->token = $token_arr[1][0];
                        }
                        if(isset($token_arr[2][0])) {
                            $this->token_secret = $token_arr[2][0];
                        }
                    }
                    if(empty($this->token) || empty($this->token_secret)) {
                        $this->error_msg[] = "Invalid access";
                    }
                }
            }
            else {
                $this->error_msg[] = "Invalid authorization";
            }
        }
		return array('token'=>$this->token,'secret'=>$this->token_secret);
    }

    /**
     * Get error message
     *
     * @access public
     * @param NIL
     * @return String
     */       
    public function get_error()
    {
        if($this->error_msg){
            return implode(", ", $this->error_msg);
        }else{
            return '';
        }
    }
}
?>
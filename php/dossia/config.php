<?php
define('SERVICE_PATH','https://dev-api.dossia.org/dossia-restful-api/services/v2.0/'); //Dossia api end point.
define('REQUEST_TOKEN_URL','https://webui1.dossia.org/authserver/request_token'); //Dossia end-point to get Request token.
define('AUTHORIZE_TOKEN_URL','https://webui1.dossia.org/authserver/authorize'); //Dossia end-point to get authorization.
define('ACCESS_TOKEN_URL','https://webui1.dossia.org/authserver/access_token'); //Dossia end-point to get Access token.

define('CUSTOMER_KEY','trigentWeight'); //Consumer key, that is registered in Dossia server.
define('CUSTOMER_SECRET','trigentSecret'); //Consumer key, that is registered in Dossia server.

//Change the following based on your environment
define('USER_DOMAIN',$_SERVER['HTTP_HOST']); 
define('USER_BASE_DIR','dossia'); 
define('USER_HTTP_METHOD','http'); 
define('USER_SIG_METHOD','HMAC-SHA1'); 
define('USER_PROXY','');
?>
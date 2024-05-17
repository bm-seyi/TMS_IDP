## 0.0.1
API has two endpoints each be able to deal with post requests entailing authentication and registration. 

## 0.0.2
API now uses API Keys for authentication, which is done by using middleware. A unnecessary assembly was removed from the DatabaseHelper.cs. A method was created in the Authentication.cs to generate API Keys. This method is currently unused but will be used in the future. 

## 0.0.3
Token Bucket rate limiting has been configured in the program.cs. Changes has been made to various files to support this. Currently the rate limiting has only been applied to the Authentication endpoint. 

## 0.0.4
Token bucket configuration readability and maintainability has been improved. Also one the rate limit has been succeeded the correct error code is returned. 

## 0.0.5
Various changes have been made to improve code readability and maintainability. Also error handling has been added to various methods. 
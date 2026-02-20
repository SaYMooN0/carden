``` 
INSERT INTO users (  
 Id,  
 Email,  
 PasswordHash,  
 RegistrationDate  
)  
VALUES (  
 $1, --UUID(UserId)  
 $2, --Email  
 $3, --PasswordHash  
 $4 --RegistrationDate  
)
```
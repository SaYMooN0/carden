```
CREATE TABLE users (
    Id UUID PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    RegistrationDate TIMESTAMP NOT NULL
);

CREATE INDEX idx_users_email ON users(Email);
```
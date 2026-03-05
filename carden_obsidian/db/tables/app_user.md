```
CREATE TABLE app_user (
    Id UUID PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    RegistrationDate TIMESTAMPTZ NOT NULL
);

CREATE INDEX idx_users_email ON app_user(Email);
```
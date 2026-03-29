# Deployed Version

A deployed version of the app can be found here: https://wonderful-moss-08ebe3c1e.2.azurestaticapps.net/

# Backend

## Requirements
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed

## Running the backend

Create a key for use for JWT signing. For development environments, this key can be anything long enough to meet the 256-bit key requirement

```bash
export JWT_KEY=my_super_secret_secure_eg_key_signing_key
```

Starting from the root folder, run the following commands:

```bash
cd backend/forecast-api
dotnet run
```

This will start up a local backend, running on `https://localhost:7241`

### Running tests

To run the test, once again starting from the root folder, run the following commands:

```bash
cd backend
dotnet test
```

Note, the `JWT_KEY` environment variable is still needed to run the test suite (in particular, for integration tests)


# Frontend

## Requirements
- Angular v21.2.3
- Node v25.8.2
- Backend previously started

## Running the frontend

Starting from the root folder, run the following commands:

```bash
cd frontend
npm start
```

This will start up a local frontend, running on `http://localhost:4200/`

# Usage

1. Navigate to the frontend start page at http://localhost:4200/
2. Login to the application
    * The current login logic is very simple - any username is accepted, so long as the password matches a simple hardcoded value - `Sup3rSe_cr3tP@ssw0rd`
3. You will be taken to the main page. The app will by default use your current IP address to fetch your current location and display the results
    * In development, this IP address lookup won't resolve properly as the request isn't coming in over the internet.
    * Instead, there is a configuration value called `FallbackIPAddress` set in `backend/forecast-api/appsettings.json`. This is by default set to be an IP address in Toronto, but it can be changed to any other IP address you like to try out different geolocation functionality
        * If this is changed, you must restart the backend
4. You can optionally select from a limited set of avaialable cities to see info and forecast data about them as well
5. You can optionally select the date for which you want to receive forecasts by using the date picker. 
    * You can select dates up to 5 days in the future
    * The app will return 8 days of forecast data
6. Logging out can be done by clicking the "Logout" button in the top right corner

# Known Limitations

* Due to the reliance on external systems for data (and remaining on free usage plans for most of the APIs), this loading of data can be slow or error-prone
    * The API used for geolocation of IP address will occasionally send a 429 Too Many Requests
        * A rudimentary caching system has been put in place to get around this, but it has its own limitations (e.g. thread safety)
    * The API used for fetching forecast data can be very slow to respond, and will sometimes timeout
        * This is particularly notable in the deployed service. Further investigation is needed to determine why.

* Due to the nature of how Wikipedia queries are made, if the geo-located city name can't be used in the query, the "City Data" requests will fail.

* The auth service is very limited, given that the password is a hardcoded dummy value that is the same for every user. The whole system could be fleshed out in the future, including proper password storage/federation, blacklisting logged out tokens, etc
    * Generally speaking, even hosting the auth provider within the same app is not ideal.

* The scope of available cities in the dropdown is also small, and hardcoded into the system. This would mean more cities needed to be added in the future, and that doing so would be tricky.

# AI Agent Disclosure

Copilot with Claude Haiku v4.5 was used for code generation, in particular for good chunks of the frontend

However, it did not do a _great_ job, and a lot of the code needed thorough review and manual cleanup. As a result, there is very little code in the repo that was written exclusively by agents
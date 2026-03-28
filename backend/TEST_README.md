# Forecast API Test Suite

Comprehensive test coverage for the Forecast API backend, including unit tests, integration tests, and end-to-end tests.

## Test Structure

### Unit Tests (`forecast-api.UnitTests`)

Unit tests focus on testing individual components in isolation using mocks for dependencies.

#### Services Tests
- **DummyAuthenticationServiceTests** - Authentication service validation
  - Correct password validation
  - Incorrect password handling
  - Empty/null password handling

- **InMemoryCityServiceTests** - City service functionality
  - City list retrieval
  - Geolocation lookup by city ID
  - Handling invalid city IDs
  - Verification of all city data

#### Model Conversion Tests
- **OpenMeteoModelConversionTests** - OpenMeteo external model conversion
  - JSON to forecast data conversion
  - Data accuracy preservation
  - Handling empty/large datasets

- **IPAPIModelConversionTests** - IP API external model conversion
  - JSON to geolocation conversion
  - Coordinate precision preservation
  - Support for negative coordinates (southern/western hemispheres)

#### Extension Tests
- **ContextExtensionsTests** - HTTP context extensions
  - IP address extraction from X-Forwarded-For header
  - Remote IP address detection
  - Loopback address filtering
  - IPv6 support

#### Controller Tests
- **AuthControllerTests** - Authentication endpoints
  - Login with valid/invalid credentials
  - Access token generation and expiration
  - Refresh token lifecycle
  - Token validation

- **ForecastControllerTests** - Forecast endpoints
  - City-based forecast retrieval
  - Specific date forecast
  - Handling invalid cities
  - Service error handling

- **CityDataControllerTests** - City data endpoints
  - City list retrieval
  - City data lookup by ID
  - Authorization requirements
  - Handling unavailable services

### Integration Tests (`forecast-api.IntegrationTests`)

Integration tests verify components working together, using WireMock to simulate external HTTP services without making real calls.

#### Service Integration Tests
- **IpapiGeolocationServiceIntegrationTests** - Setup for mocked IP geolocation API
  - Mock response handling
  - Error response scenarios

- **OpenMeteoForecastServiceIntegrationTests** - JSON deserialization and conversion
  - Multi-day forecast handling
  - Data type preservation
  - Extended forecast periods

- **IpapiModelDeserializationTests** - IP API JSON deserialization
  - Various country data
  - Coordinate handling

- **ForecastServiceWithMockTests** - Forecast service with WireMock
  - Mocked OpenMeteo API responses
  - Error handling
  - Response parsing

- **IpGeolocationServiceWithMockTests** - IP geolocation with WireMock
  - Mocked IPAPI responses
  - Response caching verification
  - Error responses
  - Multiple country support

- **CityDataServiceWithMockTests** - City data service with WireMock
  - Mocked Wikipedia responses
  - Citation removal
  - Population data extraction
  - 404 error handling

#### Controller Integration Tests
- **AuthControllerIntegrationTests**
  - Full authentication flow
  - Token refresh flow
  - Invalid credentials

- **CityDataControllerIntegrationTests**
  - Protected endpoint access
  - Authorization checks
  - Data retrieval

- **ForecastControllerIntegrationTests**
  - Forecast data retrieval
  - Authorization requirements
  - Date parameter handling

#### End-to-End Tests
- **AuthControllerE2ETests** - Complete auth lifecycle
  - Login flow with credential validation
  - Token usage for authentication
  - Full refresh token flow
  - Token expiration times
  - Multiple login sessions

- **CityDataControllerE2ETests** - Complete city data flow
  - City list retrieval and validation
  - All cities returned correctly
  - City data retrieval with external service mocking
  - Authorization enforcement

- **ForecastControllerE2ETests** - Complete forecast flow
  - Forecast retrieval for all valid cities
  - Future date forecasting
  - Invalid city handling
  - Authorization enforcement
  - External service resilience

## Test Coverage

### Services
- ✅ IAuthenticationService (DummyAuthenticationService)
- ✅ ICityService (InMemoryCityService)
- ✅ IForecastService (OpenMeteoForecastService)
- ✅ IIPGeolocationService (IPAPI_IPGeolocationService)
- ✅ ICityDataService (WikipediaCityDataService)

### Model Conversions
- ✅ IPAPIGeolocationData.ToGeolocation()
- ✅ OpenMeteoForecastData.toForecastData()

### Extensions
- ✅ ContextExtensions.GetIpAddress() - with support for X-Forwarded-For, loopback detection, IPv6

### Controllers
- ✅ AuthController (login, refresh endpoints)
- ✅ ForecastController (city-based, IP-based retrieval)
- ✅ CityDataController (list, city-based, IP-based retrieval)

## Running the Tests

### Run all tests
```bash
dotnet test
```

### Run only unit tests
```bash
dotnet test forecast-api.UnitTests/forecast-api.UnitTests.csproj
```

### Run only integration tests
```bash
dotnet test forecast-api.IntegrationTests/forecast-api.IntegrationTests.csproj
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~DummyAuthenticationServiceTests"
```

### Run with verbose output
```bash
dotnet test --verbosity detailed
```

### Generate coverage report
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=lcov
```

## Dependencies

### Unit Tests
- **xUnit** (v2.8.1) - Test framework
- **Moq** (v4.20.70) - Mocking library
- **Microsoft.NET.Test.Sdk** (v17.11.1) - Test SDK

### Integration Tests
- **xUnit** (v2.8.1) - Test framework
- **WireMock.Net** (v1.6.17) - HTTP service mocking
- **Microsoft.AspNetCore.Mvc.Testing** (v10.0.5) - ASP.NET Core testing utilities

## Test Mocking Strategy

### Unit Tests
- Services are mocked using Moq
- External dependencies (HttpClient) are mocked
- Focus on testing business logic in isolation

### Integration Tests

#### Service-Level Integration
- **WireMock.Net** is used to mock external HTTP services:
  - OpenMeteo API for forecast data
  - IPAPI service for geolocation
  - Wikipedia API for city data
- Services are tested with real implementations but mocked HTTP responses
- Caching behavior is verified

#### Controller-Level Integration
- **WebApplicationFactory** is used to create a test server
- Real database/services are used when not replaced by mocks
- Authentication flow is tested end-to-end
- Authorization headers are properly handled

## Key Test Scenarios

### Authentication
- ✅ Successful login with correct credentials
- ✅ Login failure with incorrect credentials
- ✅ Token generation and validation
- ✅ Refresh token lifecycle and expiration
- ✅ Old refresh tokens become invalid after refresh
- ✅ Token expiration times are correct

### Authorization
- ✅ Protected endpoints require valid JWT
- ✅ Invalid tokens are rejected
- ✅ Missing Authorization header returns 401

### Data Retrieval
- ✅ Valid city IDs return city data
- ✅ Invalid city IDs return 404
- ✅ All predefined cities are accessible
- ✅ City names and geolocation data are correct

### Forecast Data
- ✅ Forecasts are retrieved for valid cities
- ✅ Custom date parameters are respected
- ✅ Future date forecasting works
- ✅ OpenMeteo responses are properly converted

### Error Handling
- ✅ External service failures are handled gracefully
- ✅ Invalid input is rejected
- ✅ Appropriate HTTP status codes are returned
- ✅ Error responses contain meaningful information

## Mocking External Services

The integration tests demonstrate how to mock external HTTP services without making real calls:

```csharp
// Example: Mocking OpenMeteo API
_mockServer.Given(
    Request.Create()
        .WithPath("/v1/forecast")
        .UsingGet()
)
.RespondWith(
    Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", "application/json")
        .WithBody(JsonSerializer.Serialize(mockResponse))
);
```

This approach:
- Eliminates external service dependencies
- Ensures tests run consistently
- Allows testing error scenarios
- Improves test performance
- Reduces flakiness

## Best Practices

1. **Unit Tests**: Test individual methods/classes with mocked dependencies
2. **Integration Tests**: Test components working together with mocked external services
3. **E2E Tests**: Test complete workflows including authorization and data flow
4. **Isolation**: Each test should be independent and not rely on execution order
5. **Clarity**: Test names clearly describe what is being tested
6. **Arrangement**: Use AAA pattern (Arrange, Act, Assert)
7. **Coverage**: Aim for high coverage of critical paths
8. **Maintenance**: Keep tests simple and maintainable

## Notes

- External service calls (Wikipedia, OpenMeteo, IPAPI) in integration tests are handled gracefully with fallback assertions that accept either success or service unavailability
- The test suite is designed to work in CI/CD environments without external service dependencies
- All sensitive data (JWT keys, refresh tokens) are handled securely in tests
- Tests follow the xUnit convention of having public parameterless constructors for fixtures

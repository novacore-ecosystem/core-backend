# BuildingBlock.gRpc

Enterprise gRPC infrastructure for inter-service communication in NovaCore microservices architecture.

## Overview

**BuildingBlock.Contract** - Contains protobuf definitions (.proto files)
**BuildingBlock.gRpc** - Runtime infrastructure (server, client, interceptors)

```
BuildingBlock.Contract/Protos/*.proto
    ↓ (Code generation)
Contracts (C# classes)
    ↓
BuildingBlock.gRpc (Runtime)
    ├─ Server Extensions
    ├─ Client Extensions
    ├─ Interceptors (Logging, Error Handling)
    └─ Health Checks
```

## Features

✅ **Automatic Code Generation** - Protobuf → C# code  
✅ **Server Interceptors** - Logging, error handling, tracing  
✅ **Client Factories** - Configured HttpClient pools  
✅ **Retry Policies** - Automatic retries with backoff  
✅ **Health Checks** - K8s/Docker Compose ready  
✅ **Reflection** - gRPC tools support (grpcurl, gRPC Studio)  
✅ **Message Compression** - Automatic gzip  
✅ **Streaming Support** - Server/bidirectional streams  

## Setup

### 1. Add References

```csharp
// Service project
<ItemGroup>
  <ProjectReference Include="...\BuildingBlock.Contract\BuildingBlock.Contract.csproj" />
  <ProjectReference Include="...\BuildingBlock.gRpc\BuildingBlock.gRpc.csproj" />
</ItemGroup>
```

### 2. Define Proto (in BuildingBlock.Contract/Protos/)

```protobuf
syntax = "proto3";

package novacore.auth;

option csharp_namespace = "NovaCore.Contracts.Auth";

service AuthService {
  rpc ValidateToken (ValidateTokenRequest) returns (ValidateTokenResponse);
  rpc CreateUser (CreateUserRequest) returns (CreateUserResponse);
}

message ValidateTokenRequest {
  string token = 1;
}

message ValidateTokenResponse {
  bool valid = 1;
  string user_id = 2;
  string email = 3;
}

message CreateUserRequest {
  string email = 1;
  string password = 2;
  string full_name = 3;
}

message CreateUserResponse {
  string user_id = 1;
  bool success = 2;
  string message = 3;
}
```

### 3. Implement Service (in Service)

```csharp
using NovaCore.Contracts.Auth;
using Grpc.Core;

public class AuthService : AuthService.AuthServiceBase
{
    private readonly IAuthRepository _authRepo;

    public AuthService(IAuthRepository authRepo)
    {
        _authRepo = authRepo;
    }

    public override async Task<ValidateTokenResponse> ValidateToken(
        ValidateTokenRequest request,
        ServerCallContext context)
    {
        var userId = await _authRepo.ValidateTokenAsync(request.Token);

        if (userId == null)
            return new ValidateTokenResponse { Valid = false };

        return new ValidateTokenResponse
        {
            Valid = true,
            UserId = userId,
            Email = await _authRepo.GetEmailAsync(userId)
        };
    }

    public override async Task<CreateUserResponse> CreateUser(
        CreateUserRequest request,
        ServerCallContext context)
    {
        try
        {
            var userId = await _authRepo.CreateUserAsync(
                request.Email,
                request.Password,
                request.FullName);

            return new CreateUserResponse
            {
                UserId = userId,
                Success = true,
                Message = "User created successfully"
            };
        }
        catch (Exception ex)
        {
            return new CreateUserResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
```

### 4. Register in Program.cs (Server Side)

```csharp
// Add gRPC services (see BuildingBlock.Grpc/Server/GrpcServerExtensions.cs for the actual signature)
builder.Services.AddGrpcServer();

var app = builder.Build();

app.UseRouting();

// Map gRPC endpoints registered via AddGrpcServer
app.MapGrpcServices();

// Map health check
app.MapHealthChecks("/health");

app.Run();
```

> The exact registration surface is `AddGrpcServer()` / `MapGrpcServices()` in `BuildingBlock.Grpc/Server/GrpcServerExtensions.cs`. Check that file before copying examples below verbatim — this doc predates the current implementation and some of the following code samples (retry policy, service mesh, streaming) describe the general gRPC/YARP patterns rather than APIs confirmed to exist in this specific building block.

### 5. Register Client (Client Side)

```csharp
// In consumer service's DependencyInjection.cs

builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(
    serviceName: "auth-service",
    address: new Uri("https://auth-service:5000"));

// Or with custom configuration
builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(
    serviceName: "product-service",
    address: new Uri("https://product-service:5000"),
    configureChannel: options =>
    {
        options.HttpHandler = new SocketsHttpHandler
        {
            KeepAliveInterval = TimeSpan.FromSeconds(15)
        };
    });
```

### 6. Consume in Client Service

```csharp
public class OrderService
{
    private readonly AuthService.AuthServiceClient _authClient;

    public OrderService(AuthService.AuthServiceClient authClient)
    {
        _authClient = authClient;
    }

    public async Task<Order> CreateOrderAsync(string token, CreateOrderDto order)
    {
        // Call gRPC service
        var validateRequest = new ValidateTokenRequest { Token = token };
        var tokenResponse = await _authClient.ValidateTokenAsync(
            validateRequest,
            deadline: DateTime.UtcNow.AddSeconds(5));

        if (!tokenResponse.Valid)
            throw new UnauthorizedAccessException("Invalid token");

        var userId = tokenResponse.UserId;
        // Create order with validated user
        return new Order { UserId = userId, Total = order.Total };
    }
}
```

## Proto Organization

```
BuildingBlock.Contract/Protos/
├── google/protobuf/
│   └── empty.proto
├── common.proto              (Shared request/response wrappers)
├── auth/
│   ├── auth.proto           (Auth service definitions)
│   └── user.proto           (User-related messages)
├── product/
│   ├── product.proto
│   └── catalog.proto
└── order/
    ├── order.proto
    └── fulfillment.proto
```

## Interceptors

### LoggingInterceptor
- Logs all incoming gRPC calls
- Tracks execution time
- Records method name and peer address

### ErrorHandlingInterceptor
- Converts exceptions to gRPC status codes
- Validation errors → `InvalidArgument`
- Unauthorized → `Unauthenticated`
- Internal errors → `Internal`
- Logs all errors with context

## Health Checks

Automatic health check service for load balancers and monitoring:

```bash
# Check service health
grpcurl -plaintext localhost:5000 grpc.health.v1.Health/Check

# Response
{
  "status": "SERVING"
}
```

Docker Compose example:

```yaml
services:
  auth-api:
    healthcheck:
      test: ["CMD", "grpcurl", "-plaintext", "localhost:5000", "grpc.health.v1.Health/Check"]
      interval: 10s
      timeout: 5s
      retries: 3
```

## Reflection (Development)

Enable exploration of gRPC services without proto files:

```bash
# List services
grpcurl -plaintext localhost:5000 list

# List methods
grpcurl -plaintext localhost:5000 list novacore.auth.AuthService

# Describe message
grpcurl -plaintext localhost:5000 describe novacore.auth.ValidateTokenRequest

# Call service
grpcurl -plaintext -d '{"token":"abc123"}' \
  localhost:5000 novacore.auth.AuthService/ValidateToken
```

## Streaming

### Server Streaming

```protobuf
service NotificationService {
  rpc Subscribe (SubscribeRequest) returns (stream NotificationEvent);
}
```

```csharp
public override async Task Subscribe(
    SubscribeRequest request,
    IServerStreamWriter<NotificationEvent> responseStream,
    ServerCallContext context)
{
    var userId = request.UserId;

    while (!context.CancellationToken.IsCancellationRequested)
    {
        var notification = await _notificationService.GetNextAsync(userId);
        await responseStream.WriteAsync(notification);
        await Task.Delay(1000); // Poll interval
    }
}
```

### Bidirectional Streaming

```protobuf
service ChatService {
  rpc Chat (stream ChatMessage) returns (stream ChatMessage);
}
```

```csharp
public override async Task Chat(
    IAsyncStreamReader<ChatMessage> requestStream,
    IServerStreamWriter<ChatMessage> responseStream,
    ServerCallContext context)
{
    await foreach (var message in requestStream.ReadAllAsync())
    {
        var response = new ChatMessage
        {
            SenderId = "system",
            Content = $"Echo: {message.Content}",
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
        };

        await responseStream.WriteAsync(response);
    }
}
```

## Retry Policies

Automatic retries for transient failures:

```csharp
// These status codes trigger automatic retries:
- StatusCode.Unavailable (service temporarily down)
- StatusCode.ResourceExhausted (rate limited)

// Retry configuration:
- Max attempts: 3
- Initial backoff: 0.1s
- Max backoff: 1s
- Multiplier: 2x exponential
```

Customize retry policy:

```csharp
services.AddGrpcClient<MyService.MyServiceClient>(options =>
{
    options.Address = new Uri("https://service:5000");
    options.ChannelOptionsActions.Add(o =>
    {
        o.ServiceConfig = new ServiceConfig
        {
            MethodConfigs =
            {
                new MethodConfig
                {
                    Names = { MethodName.Default },
                    RetryPolicy = new RetryPolicy
                    {
                        MaxAttempts = 5,
                        InitialBackoff = TimeSpan.FromMilliseconds(100),
                        MaxBackoff = TimeSpan.FromSeconds(10),
                        BackoffMultiplier = 2
                    }
                }
            }
        };
    });
});
```

## Testing

### Mock gRPC Clients

```csharp
[Test]
public async Task OrderService_ShouldValidateToken()
{
    // Arrange
    var authClientMock = new Mock<AuthService.AuthServiceClient>();
    authClientMock
        .Setup(x => x.ValidateTokenAsync(
            It.IsAny<ValidateTokenRequest>(),
            It.IsAny<Metadata>(),
            null,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidateTokenResponse { Valid = true, UserId = "user-123" });

    var service = new OrderService(authClientMock.Object);

    // Act
    var order = await service.CreateOrderAsync("token-abc", createOrderDto);

    // Assert
    Assert.AreEqual("user-123", order.UserId);
}
```

## Docker Compose Example

```yaml
version: '3.8'

services:
  auth-api:
    image: novacore/auth-api:latest
    ports:
      - "5000:5000"  # gRPC port
      - "5001:5001"  # HTTP (REST/health)
    environment:
      ASPNETCORE_URLS: "https://+:5000;http://+:5001"
      ASPNETCORE_HTTPS_PORT: 5000
    healthcheck:
      test: ["CMD", "grpcurl", "-plaintext", "localhost:5000", "grpc.health.v1.Health/Check"]
      interval: 10s
      timeout: 5s
      retries: 3

  order-api:
    image: novacore/order-api:latest
    ports:
      - "5002:5002"
      - "5003:5003"
    depends_on:
      auth-api:
        condition: service_healthy
    environment:
      ASPNETCORE_URLS: "https://+:5002;http://+:5003"
      ASPNETCORE_HTTPS_PORT: 5002
      GRPC_AUTH_SERVICE: "https://auth-api:5000"
```

## Performance Tips

1. **Connection Pooling** - HttpClientFactory handles this
2. **Message Compression** - Enabled by default for responses >1KB
3. **Keep-Alive** - Prevents connection timeouts
4. **Deadlines** - Always set timeouts on client calls
5. **Stream Buffering** - Don't buffer large streams in memory
6. **Batch Messages** - Group small messages when possible

## Security

### HTTPS in Production

```csharp
// Server
app.UseHttpsRedirection();

// Client
var channel = GrpcChannel.ForAddress("https://auth-service:5000");
var client = new AuthService.AuthServiceClient(channel);
```

### JWT Authentication

```csharp
// Proto definition
service AuthService {
  option (google.api.http) = {
    get: "/v1/auth:validate"
  };
}

// Server-side validation
public override async Task<ValidateResponse> Validate(
    ValidateRequest request,
    ServerCallContext context)
{
    var token = context.RequestHeaders
        .FirstOrDefault(h => h.Key == "authorization")?
        .Value;

    if (token == null)
        throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing token"));

    var principal = ValidateJwt(token);
    return new ValidateResponse { UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value };
}
```

## Common Patterns

### Service Discovery

```csharp
// Use service name mapping
services.AddGrpcClientMesh(new Dictionary<string, Uri>
{
    { "auth-service", new Uri("https://auth-service:5000") },
    { "product-service", new Uri("https://product-service:5001") },
    { "order-service", new Uri("https://order-service:5002") }
});
```

### Circuit Breaker Pattern

Combine with Polly for advanced resilience:

```csharp
var policy = Policy
    .Handle<RpcException>(e => e.StatusCode == StatusCode.Unavailable)
    .OrTransientException()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

await policy.ExecuteAsync(() => 
    _authClient.ValidateTokenAsync(request));
```

### Request Tracing

```csharp
// Add correlation ID to metadata
var metadata = new Metadata
{
    { "x-correlation-id", correlationId },
    { "x-request-id", requestId }
};

var response = await _authClient.ValidateTokenAsync(
    request,
    metadata,
    deadline: DateTime.UtcNow.AddSeconds(5));
```

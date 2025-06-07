# SignalR Real-Time Messaging Integration Guide

## Overview
This guide explains how to set up and use the SignalR real-time messaging system in the Thrume social network application.

## Prerequisites
- .NET 8.0 SDK
- Node.js 18+ with pnpm
- Valid SSL certificates (localhost.pem and localhost-key.pem in frontend directory)

## Backend Setup

### 1. SignalR Hub Configuration
The backend includes a `ChatHub` at `/chathub` with the following methods:
- `JoinConversationAsync(conversationId)` - Join a conversation room
- `LeaveConversationAsync(conversationId)` - Leave a conversation room  
- `SendTypingIndicatorAsync(conversationId)` - Broadcast typing status
- `StopTypingIndicatorAsync(conversationId)` - Stop typing broadcast

### 2. Authentication
SignalR uses the same cookie authentication as the main API. Users must be logged in to connect.

### 3. Rate Limiting
SignalR includes rate limiting via `SignalRRateLimitFilter` to prevent abuse.

## Frontend Setup

### 1. Package Dependencies
Ensure `@microsoft/signalr` is installed:
```json
{
  "dependencies": {
    "@microsoft/signalr": "^8.0.7"
  }
}
```

### 2. Service Configuration
The `signalRService` handles all SignalR communication:
- Automatic reconnection with exponential backoff
- Event-driven architecture
- Graceful fallback to REST API when disconnected

### 3. Store Integration
The `messageStore` (Pinia) integrates SignalR events with the application state:
- Real-time message updates
- Typing indicators
- User presence tracking
- Conversation management

## Usage

### Starting the Application

1. **Start Backend:**
```bash
cd backend/Thrume
dotnet run --project Thrume.Api
```

2. **Start Frontend:**
```bash
cd frontend/thrume-frontend
pnpm dev
```

### Connection Flow

1. User logs in through normal authentication
2. Navigate to messages (`/messages`)
3. SignalR automatically connects using existing authentication
4. Join specific conversations to receive real-time updates

### Real-Time Features

- **Instant Messaging**: Messages appear immediately for all conversation participants
- **Typing Indicators**: See when others are typing
- **User Presence**: Know who's online
- **Connection Status**: Visual indicator of connection state
- **Automatic Reconnection**: Handles network interruptions gracefully

## Error Handling

### Connection Failures
- Automatic retry with exponential backoff
- Fallback to REST API for message sending
- User notification of connection status

### Authentication Errors
- Redirects to login if session expires
- Graceful degradation of features

### Rate Limiting
- 429 errors handled gracefully
- Temporary message sending delays

## Testing

### Manual Testing
1. Open two browser windows/tabs
2. Log in as different users
3. Start a conversation
4. Verify real-time message delivery
5. Test typing indicators
6. Test connection resilience (disable network briefly)

### Error Scenarios
- Network disconnection
- Server restart
- Authentication expiry
- Rate limit exceeded

## Configuration

### Vite Proxy Configuration
```typescript
proxy: {
  '/chathub': {
    target: 'https://localhost:7169',
    changeOrigin: true,
    secure: false,
    ws: true, // Enable WebSocket proxying
  }
}
```

### SignalR Hub Options
```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 32768; // 32KB
    options.MaximumParallelInvocationsPerClient = 1;
});
```

## Troubleshooting

### Common Issues

1. **Connection Failed**: Check CORS settings and SSL certificates
2. **Authentication Issues**: Verify cookie settings and authentication state
3. **Messages Not Appearing**: Check console for SignalR errors
4. **Typing Indicators Not Working**: Verify method names match between frontend/backend

### Debug Logs
Enable SignalR logging in browser console:
```typescript
.configureLogging(LogLevel.Debug)
```

## Security Considerations

- All SignalR connections require authentication
- Users can only join conversations they participate in
- Rate limiting prevents message flooding
- HTTPS required for production

## Performance

- WebSocket connections maintained efficiently
- Automatic cleanup on disconnect
- Minimal message overhead
- Optimized for real-time communication

## Future Enhancements

- Message read receipts
- File sharing through SignalR
- Voice/video call signaling
- Group chat support
- Message encryption
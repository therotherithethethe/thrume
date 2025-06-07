export interface ConnectionState {
  status: 'connected' | 'connecting' | 'disconnected' | 'reconnecting'
  reconnectAttempts: number
  error?: string
  lastConnected?: Date
}

export interface TypingIndicator {
  userId: string
  conversationId: string
  isTyping: boolean
  timestamp: Date
}

export interface UserPresence {
  userId: string
  isOnline: boolean
  lastSeen?: Date
}

export interface SignalRMessage {
  Id: string
  ConversationId: string
  SenderId: string
  SenderUserName: string
  SenderPictureUrl: string | null
  Content: string
  SentAt: string
}

// WebRTC Calling Types
export interface CallOffer {
  callId: string
  callerId: string
  callerUsername: string
  conversationId: string
  callType: CallType
  offer?: RTCSessionDescriptionInit | null // Made optional since backend sends null initially
  timestamp: string
}

export interface CallAnswer {
  callId: string
  answer: RTCSessionDescriptionInit
}

export interface CallIceCandidate {
  callId: string
  candidate: RTCIceCandidateInit
}

export interface CallRejection {
  callId: string
  reason?: string
}

export interface CallTermination {
  callId: string
  reason?: string
}

export interface CallStateUpdate {
  callId: string
  state: CallState
  timestamp: string
}

export enum CallType {
  Voice = 'voice',
  Video = 'video'
}

export enum CallState {
  Idle = 'idle',
  Initiating = 'initiating',
  Ringing = 'ringing',
  Connecting = 'connecting',
  Connected = 'connected',
  Reconnecting = 'reconnecting',
  Ended = 'ended',
  Failed = 'failed'
}

export interface MediaState {
  isMuted: boolean
  isCameraOn: boolean
  isScreenSharing: boolean
}

export interface CallPermissions {
  camera: boolean
  microphone: boolean
  screenShare: boolean
}

export interface Call {
  id: string
  callerId: string
  calleeId: string
  callerUsername: string
  calleeUsername?: string
  callType: CallType
  state: CallState
  conversationId: string
  startedAt: Date
  endedAt?: Date
  mediaState: MediaState
}

export interface SignalREvents {
  // Incoming events from server
  ReceiveMessage: (message: SignalRMessage) => void
  ReceiveTypingIndicator: (indicator: TypingIndicator) => void
  UserJoined: (presence: UserPresence) => void
  UserLeft: (presence: UserPresence) => void
  MessageRead: (messageId: string, userId: string) => void
  
  // Call events
  IncomingCall: (callOffer: CallOffer) => void
  CallAccepted: (callId: string) => void
  CallRejected: (callRejection: CallRejection) => void
  CallEnded: (callTermination: CallTermination) => void
  ReceiveOffer: (callOffer: CallOffer) => void
  ReceiveAnswer: (callAnswer: CallAnswer) => void
  ReceiveIceCandidate: (iceCandidate: CallIceCandidate) => void
  CallStateChanged: (callStateUpdate: CallStateUpdate) => void
  
  // Outgoing events to server
  JoinConversation: (conversationId: string) => void
  LeaveConversation: (conversationId: string) => void
  StartTyping: (conversationId: string) => void
  StopTyping: (conversationId: string) => void
  MarkMessageAsRead: (messageId: string) => void
  
  // Call control events
  InitiateCall: (calleeId: string, callType: CallType, conversationId: string) => void
  AcceptCall: (callId: string) => void
  RejectCall: (callId: string, reason?: string) => void
  EndCall: (callId: string) => void
  SendOffer: (callId: string, offer: RTCSessionDescriptionInit) => void
  SendAnswer: (callId: string, answer: RTCSessionDescriptionInit) => void
  SendIceCandidate: (callId: string, candidate: RTCIceCandidateInit) => void
}

export interface ReconnectionConfig {
  maxRetries: number
  baseDelay: number
  maxDelay: number
  jitter: boolean
}
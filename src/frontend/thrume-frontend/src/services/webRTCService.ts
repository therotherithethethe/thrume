import { CallType, type MediaState, type CallPermissions } from '../types/signalr'

export interface WebRTCConfiguration {
  iceServers: RTCIceServer[]
  iceCandidatePoolSize: number
}

export interface MediaConstraints {
  video: boolean | MediaTrackConstraints
  audio: boolean | MediaTrackConstraints
}

export class WebRTCService {
  private peerConnection: RTCPeerConnection | null = null
  private localStream: MediaStream | null = null
  private remoteStream: MediaStream | null = null
  private configuration: WebRTCConfiguration
  private callId: string | null = null
  
  // Event callbacks
  private onLocalStreamCallback: ((stream: MediaStream) => void) | null = null
  private onRemoteStreamCallback: ((stream: MediaStream) => void) | null = null
  private onIceCandidateCallback: ((candidate: RTCIceCandidate) => void) | null = null
  private onConnectionStateChangeCallback: ((state: RTCPeerConnectionState) => void) | null = null

  constructor() {
    this.configuration = {
      iceServers: [
        { urls: 'stun:stun.l.google.com:19302' },
        { urls: 'stun:stun1.l.google.com:19302' },
        { urls: 'stun:stun2.l.google.com:19302' },
        { urls: 'stun:stun3.l.google.com:19302' },
        { urls: 'stun:stun4.l.google.com:19302' }
      ],
      iceCandidatePoolSize: 10
    }
  }

  // Event handler setters
  onLocalStream(callback: (stream: MediaStream) => void): void {
    this.onLocalStreamCallback = callback
  }

  onRemoteStream(callback: (stream: MediaStream) => void): void {
    this.onRemoteStreamCallback = callback
  }

  onIceCandidate(callback: (candidate: RTCIceCandidate) => void): void {
    this.onIceCandidateCallback = callback
  }

  onConnectionStateChange(callback: (state: RTCPeerConnectionState) => void): void {
    this.onConnectionStateChangeCallback = callback
  }

  // Initialize peer connection
  async initializePeerConnection(callId: string): Promise<void> {
    this.callId = callId
    this.peerConnection = new RTCPeerConnection(this.configuration)

    // Set up event handlers
    this.peerConnection.onicecandidate = (event) => {
      if (event.candidate && this.onIceCandidateCallback) {
        this.onIceCandidateCallback(event.candidate)
      }
    }

    this.peerConnection.ontrack = (event) => {
      const [remoteStream] = event.streams
      this.remoteStream = remoteStream
      if (this.onRemoteStreamCallback) {
        this.onRemoteStreamCallback(remoteStream)
      }
    }

    this.peerConnection.onconnectionstatechange = () => {
      if (this.peerConnection && this.onConnectionStateChangeCallback) {
        this.onConnectionStateChangeCallback(this.peerConnection.connectionState)
      }
    }

    this.peerConnection.oniceconnectionstatechange = () => {
      if (this.peerConnection) {
        console.log('ICE connection state:', this.peerConnection.iceConnectionState)
      }
    }
  }

  // Get user media (camera/microphone)
  async getUserMedia(callType: CallType): Promise<MediaStream> {
    const constraints: MediaStreamConstraints = {
      audio: {
        echoCancellation: true,
        noiseSuppression: true,
        autoGainControl: true
      },
      video: callType === CallType.Video ? {
        width: { ideal: 1280 },
        height: { ideal: 720 },
        frameRate: { ideal: 30 }
      } : false
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia(constraints)
      this.localStream = stream
      
      // Add tracks to peer connection
      if (this.peerConnection) {
        stream.getTracks().forEach(track => {
          this.peerConnection!.addTrack(track, stream)
        })
      }

      if (this.onLocalStreamCallback) {
        this.onLocalStreamCallback(stream)
      }

      return stream
    } catch (error) {
      console.error('Error accessing user media:', error)
      throw new Error('Failed to access camera/microphone')
    }
  }

  // Get display media (screen sharing)
  async getDisplayMedia(): Promise<MediaStream> {
    try {
      const stream = await navigator.mediaDevices.getDisplayMedia({
        video: {
          width: { ideal: 1920 },
          height: { ideal: 1080 },
          frameRate: { ideal: 30 }
        },
        audio: true
      })

      // Replace video track if screen sharing
      if (this.peerConnection && this.localStream) {
        const videoTrack = stream.getVideoTracks()[0]
        const sender = this.peerConnection.getSenders().find(s => 
          s.track && s.track.kind === 'video'
        )
        
        if (sender) {
          await sender.replaceTrack(videoTrack)
        }
      }

      return stream
    } catch (error) {
      console.error('Error accessing display media:', error)
      throw new Error('Failed to access screen sharing')
    }
  }

  // Create offer
  async createOffer(): Promise<RTCSessionDescriptionInit> {
    if (!this.peerConnection) {
      throw new Error('Peer connection not initialized')
    }

    try {
      const offer = await this.peerConnection.createOffer({
        offerToReceiveAudio: true,
        offerToReceiveVideo: true
      })
      
      await this.peerConnection.setLocalDescription(offer)
      return offer
    } catch (error) {
      console.error('Error creating offer:', error)
      throw new Error('Failed to create offer')
    }
  }

  // Handle incoming offer
  async handleOffer(offer: RTCSessionDescriptionInit): Promise<RTCSessionDescriptionInit> {
    if (!this.peerConnection) {
      throw new Error('Peer connection not initialized')
    }

    try {
      await this.peerConnection.setRemoteDescription(offer)
      const answer = await this.peerConnection.createAnswer()
      await this.peerConnection.setLocalDescription(answer)
      return answer
    } catch (error) {
      console.error('Error handling offer:', error)
      throw new Error('Failed to handle offer')
    }
  }

  // Handle incoming answer
  async handleAnswer(answer: RTCSessionDescriptionInit): Promise<void> {
    if (!this.peerConnection) {
      throw new Error('Peer connection not initialized')
    }

    try {
      await this.peerConnection.setRemoteDescription(answer)
    } catch (error) {
      console.error('Error handling answer:', error)
      throw new Error('Failed to handle answer')
    }
  }

  // Add ICE candidate
  async addIceCandidate(candidate: RTCIceCandidateInit): Promise<void> {
    if (!this.peerConnection) {
      throw new Error('Peer connection not initialized')
    }

    try {
      await this.peerConnection.addIceCandidate(candidate)
    } catch (error) {
      console.error('Error adding ICE candidate:', error)
      throw new Error('Failed to add ICE candidate')
    }
  }

  // Media controls
  toggleMute(): boolean {
    if (!this.localStream) return false
    
    const audioTrack = this.localStream.getAudioTracks()[0]
    if (audioTrack) {
      audioTrack.enabled = !audioTrack.enabled
      return !audioTrack.enabled // Return muted state
    }
    return false
  }

  toggleCamera(): boolean {
    if (!this.localStream) return false
    
    const videoTrack = this.localStream.getVideoTracks()[0]
    if (videoTrack) {
      videoTrack.enabled = !videoTrack.enabled
      return videoTrack.enabled // Return camera on state
    }
    return false
  }

  async toggleScreenShare(): Promise<boolean> {
    try {
      if (!this.peerConnection || !this.localStream) return false

      const videoSender = this.peerConnection.getSenders().find(sender =>
        sender.track && sender.track.kind === 'video'
      )

      if (!videoSender) return false

      const currentTrack = videoSender.track
      const isScreenShare = currentTrack && currentTrack.label.includes('screen')

      if (isScreenShare) {
        // Switch back to camera
        const cameraStream = await navigator.mediaDevices.getUserMedia({
          video: { width: { ideal: 1280 }, height: { ideal: 720 } }
        })
        const cameraTrack = cameraStream.getVideoTracks()[0]
        await videoSender.replaceTrack(cameraTrack)
        
        // Update local stream
        const audioTrack = this.localStream.getAudioTracks()[0]
        this.localStream = new MediaStream([cameraTrack, audioTrack])
        
        if (this.onLocalStreamCallback) {
          this.onLocalStreamCallback(this.localStream)
        }
        
        return false
      } else {
        // Switch to screen share
        const screenStream = await this.getDisplayMedia()
        const screenTrack = screenStream.getVideoTracks()[0]
        await videoSender.replaceTrack(screenTrack)
        
        // Update local stream
        const audioTrack = this.localStream.getAudioTracks()[0]
        this.localStream = new MediaStream([screenTrack, audioTrack])
        
        if (this.onLocalStreamCallback) {
          this.onLocalStreamCallback(this.localStream)
        }
        
        return true
      }
    } catch (error) {
      console.error('Error toggling screen share:', error)
      throw new Error('Failed to toggle screen share')
    }
  }

  // Get current media state
  getMediaState(): MediaState {
    if (!this.localStream) {
      return {
        isMuted: true,
        isCameraOn: false,
        isScreenSharing: false
      }
    }

    const audioTrack = this.localStream.getAudioTracks()[0]
    const videoTrack = this.localStream.getVideoTracks()[0]
    
    return {
      isMuted: !audioTrack?.enabled,
      isCameraOn: videoTrack?.enabled || false,
      isScreenSharing: videoTrack?.label.includes('screen') || false
    }
  }

  // End call and cleanup
  async endCall(): Promise<void> {
    try {
      // Stop all local tracks
      if (this.localStream) {
        this.localStream.getTracks().forEach(track => track.stop())
        this.localStream = null
      }

      // Close peer connection
      if (this.peerConnection) {
        this.peerConnection.close()
        this.peerConnection = null
      }

      // Clear remote stream
      this.remoteStream = null
      this.callId = null
      
      console.log('WebRTC call ended and cleaned up')
    } catch (error) {
      console.error('Error ending call:', error)
    }
  }

  // Getters
  get currentCallId(): string | null {
    return this.callId
  }

  get currentLocalStream(): MediaStream | null {
    return this.localStream
  }

  get currentRemoteStream(): MediaStream | null {
    return this.remoteStream
  }

  get connectionState(): RTCPeerConnectionState | null {
    return this.peerConnection?.connectionState || null
  }
}

// Export singleton instance
export const webRTCService = new WebRTCService()
export default webRTCService
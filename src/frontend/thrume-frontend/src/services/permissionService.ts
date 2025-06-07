import type { CallPermissions } from '../types/signalr'

export interface PermissionResult {
  granted: boolean
  error?: string
}

export class PermissionService {
  private permissions: CallPermissions = {
    camera: false,
    microphone: false,
    screenShare: false
  }

  // Check current permission status
  async checkPermissions(): Promise<CallPermissions> {
    try {
      // Check microphone permission
      const micPermission = await navigator.permissions.query({ name: 'microphone' as PermissionName })
      this.permissions.microphone = micPermission.state === 'granted'

      // Check camera permission
      const cameraPermission = await navigator.permissions.query({ name: 'camera' as PermissionName })
      this.permissions.camera = cameraPermission.state === 'granted'

      // Screen share permission is handled differently (no persistent permission)
      this.permissions.screenShare = true

      return { ...this.permissions }
    } catch (error) {
      console.warn('Permission API not supported, will check at runtime:', error)
      return { ...this.permissions }
    }
  }

  // Request microphone permission
  async requestMicrophonePermission(): Promise<PermissionResult> {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
      
      // Stop the test stream
      stream.getTracks().forEach(track => track.stop())
      
      this.permissions.microphone = true
      return { granted: true }
    } catch (error) {
      console.error('Microphone permission denied:', error)
      this.permissions.microphone = false
      
      if (error instanceof DOMException) {
        switch (error.name) {
          case 'NotAllowedError':
            return { granted: false, error: 'Microphone access denied by user' }
          case 'NotFoundError':
            return { granted: false, error: 'No microphone found' }
          case 'NotReadableError':
            return { granted: false, error: 'Microphone is already in use' }
          default:
            return { granted: false, error: 'Failed to access microphone' }
        }
      }
      
      return { granted: false, error: 'Unknown error accessing microphone' }
    }
  }

  // Request camera permission
  async requestCameraPermission(): Promise<PermissionResult> {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ video: true })
      
      // Stop the test stream
      stream.getTracks().forEach(track => track.stop())
      
      this.permissions.camera = true
      return { granted: true }
    } catch (error) {
      console.error('Camera permission denied:', error)
      this.permissions.camera = false
      
      if (error instanceof DOMException) {
        switch (error.name) {
          case 'NotAllowedError':
            return { granted: false, error: 'Camera access denied by user' }
          case 'NotFoundError':
            return { granted: false, error: 'No camera found' }
          case 'NotReadableError':
            return { granted: false, error: 'Camera is already in use' }
          default:
            return { granted: false, error: 'Failed to access camera' }
        }
      }
      
      return { granted: false, error: 'Unknown error accessing camera' }
    }
  }

  // Request screen share permission
  async requestScreenSharePermission(): Promise<PermissionResult> {
    try {
      const stream = await navigator.mediaDevices.getDisplayMedia({
        video: true,
        audio: true
      })
      
      // Stop the test stream
      stream.getTracks().forEach(track => track.stop())
      
      this.permissions.screenShare = true
      return { granted: true }
    } catch (error) {
      console.error('Screen share permission denied:', error)
      this.permissions.screenShare = false
      
      if (error instanceof DOMException) {
        switch (error.name) {
          case 'NotAllowedError':
            return { granted: false, error: 'Screen sharing denied by user' }
          case 'NotSupportedError':
            return { granted: false, error: 'Screen sharing not supported' }
          default:
            return { granted: false, error: 'Failed to access screen sharing' }
        }
      }
      
      return { granted: false, error: 'Unknown error accessing screen sharing' }
    }
  }

  // Request both camera and microphone permissions
  async requestAVPermissions(): Promise<PermissionResult> {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ 
        audio: true, 
        video: true 
      })
      
      // Stop the test stream
      stream.getTracks().forEach(track => track.stop())
      
      this.permissions.camera = true
      this.permissions.microphone = true
      return { granted: true }
    } catch (error) {
      console.error('A/V permissions denied:', error)
      
      if (error instanceof DOMException) {
        switch (error.name) {
          case 'NotAllowedError':
            return { granted: false, error: 'Camera and microphone access denied by user' }
          case 'NotFoundError':
            return { granted: false, error: 'Camera or microphone not found' }
          case 'NotReadableError':
            return { granted: false, error: 'Camera or microphone is already in use' }
          default:
            return { granted: false, error: 'Failed to access camera and microphone' }
        }
      }
      
      return { granted: false, error: 'Unknown error accessing camera and microphone' }
    }
  }

  // Check if WebRTC is supported
  isWebRTCSupported(): boolean {
    return !!(
      window.RTCPeerConnection &&
      navigator.mediaDevices &&
      navigator.mediaDevices.getUserMedia
    )
  }

  // Check if screen sharing is supported
  isScreenSharingSupported(): boolean {
    return !!(
      navigator.mediaDevices &&
      navigator.mediaDevices.getDisplayMedia
    )
  }

  // Get current permissions
  getCurrentPermissions(): CallPermissions {
    return { ...this.permissions }
  }

  // Reset permissions (useful for testing)
  resetPermissions(): void {
    this.permissions = {
      camera: false,
      microphone: false,
      screenShare: false
    }
  }
}

// Export singleton instance
export const permissionService = new PermissionService()
export default permissionService
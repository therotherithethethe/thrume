<template>
  <div class="container">
    <h2>WebRTC Media Sharing (Audio + Screen)</h2>
    <div id="connection-status">Status: {{ connectionStatus }}</div>
    <hr />

    <div>
      <label for="my-connection-id">My Connection ID:</label>
      <span id="my-connection-id">{{ myConnectionId || 'Not connected' }}</span>
    </div>
    <div>
      <span id="mic-status">Mic: {{ micStatus }}</span>
    </div>
    <div>
      <span id="screen-status">Screen: {{ screenStatus }}</span>
    </div>
    <hr />
    <div>
      <label for="target-connection-id">Target Connection ID:</label>
      <input
        id="target-connection-id"
        v-model="targetConnectionId"
        type="text"
        placeholder="Enter peer's ID"
        :disabled="isCallActive"
      />
    </div>
    <button
      id="request-mic-button"
      :disabled="!isSignalRConnected || isMicActive"
      @click="requestMicrophoneAccess"
    >
      1a. Request Mic Access
    </button>
    <button
      id="share-screen-button"
      :disabled="!isSignalRConnected || isScreenActive"
      @click="requestScreenShare"
    >
      1b. Share Screen
    </button>
    <button id="start-call-button" :disabled="!canStartCall" @click="startCall">
      2. Start Sharing
    </button>
    <button id="hang-up-button" :disabled="!isCallActive" @click="hangUp">
      Hang Up
    </button>

    <div id="call-status">Call Status: {{ callStatus }}</div>
    <hr />

    <div>
      <h4>Local Audio (Your Mic - Muted)</h4>
      <audio ref="localAudioElem" muted autoplay playsinline></audio>
    </div>
    <div>
      <h4>Remote Audio (Peer's Audio)</h4>
      <audio ref="remoteAudioElem" autoplay playsinline></audio>
    </div>
    <div>
      <h4>Remote Screen (Peer's Screen)</h4>
      <video ref="remoteVideoElem" autoplay playsinline></video>
    </div>
  </div>
</template>

<style scoped>
/* Scoped styles from the original HTML for this component */
.container {
  max-width: 600px;
  margin: 20px auto;
  padding: 15px;
  border: 1px solid #ccc;
}
button {
  margin: 5px;
  padding: 8px 12px;
}
#connection-status, #call-status, #mic-status, #screen-status {
  margin-top: 5px;
  font-weight: bold;
  display: block;
}
label {
  margin-right: 5px;
}
input[type="text"] {
  padding: 5px;
  margin-right: 10px;
}
audio, video {
  display: block;
  margin-top: 10px;
  border: 1px solid #eee;
  background-color: #f0f0f0;
  max-width: 100%;
}
video {
  min-height: 200px;
}
</style>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';

// --- Configuration ---
const peerConnectionConfig: RTCConfiguration = {
  iceServers: [
    { urls: 'stun:stun.l.google.com:19302' },
    { urls: 'stun:stun1.l.google.com:19302' },
  ],
};

// --- Template Refs (for DOM elements) ---
const localAudioElem = ref<HTMLAudioElement | null>(null);
const remoteAudioElem = ref<HTMLAudioElement | null>(null);
const remoteVideoElem = ref<HTMLVideoElement | null>(null);

// --- Reactive State ---
const signalrConnection = ref<HubConnection | null>(null);
const peerConnection = ref<RTCPeerConnection | null>(null);
const micStream = ref<MediaStream | null>(null);
const screenStream = ref<MediaStream | null>(null);

const myConnectionId = ref<string | null>(null);
const targetConnectionId = ref<string>(''); // For the v-model input
let peerConnectionTargetId: string | null = null; // The actual target of the active call

// --- Statuses ---
const connectionStatus = ref('Disconnected');
const callStatus = ref('Idle');
const micStatus = ref('Inactive');
const screenStatus = ref('Inactive');

// --- Computed Properties (for UI logic) ---
const isSignalRConnected = computed(
  () => {
    console.log('HHHHHHHH' + signalrConnection.value?.state)
    return signalrConnection.value?.state === HubConnectionState.Connecting
  }
);
const isMicActive = computed(() => !!micStream.value);
const isScreenActive = computed(() => !!screenStream.value);
const isCallActive = computed(() => !!peerConnection.value);
const canStartCall = computed(
  () =>
    isSignalRConnected.value &&
    (isMicActive.value || isScreenActive.value) &&
    !isCallActive.value &&
    targetConnectionId.value.length > 0
);


// --- SignalR Methods ---
const setupSignalR = () => {
  const connection = new HubConnectionBuilder()
    .withUrl('https://thrume-api.onrender.com/voiceCallHub',) // Replace with your SignalR Hub URL
    .configureLogging(LogLevel.Information)
    .build();

  connection.on('ReceiveOffer', handleOffer);
  connection.on('ReceiveAnswer', handleAnswer);
  connection.on('ReceiveIceCandidate', handleIceCandidate);

  connection.onclose(() => {
    console.log('SignalR Disconnected.');
    connectionStatus.value = 'Disconnected';
    myConnectionId.value = null;
    hangUp(); // Clean up call state if SignalR disconnects
  });

  connection
    .start()
    .then(() => {
      myConnectionId.value = connection.connectionId;
      console.log('SignalR Connected. My Connection ID:', myConnectionId.value);
      connectionStatus.value = `Connected`;
    })
    .catch((err) => {
      console.error('SignalR Connection Error: ', err);
      connectionStatus.value = 'Connection Failed';
    });

  signalrConnection.value = connection;
};

// --- Media Methods ---
const requestMicrophoneAccess = async () => {
  if (micStream.value) return;
  console.log('Requesting microphone access...');
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
    console.log('Microphone access granted.');
    micStream.value = stream;
    if (localAudioElem.value) localAudioElem.value.srcObject = stream;
    micStatus.value = 'Active';
  } catch (error) {
    console.error('Error accessing microphone:', error);
    micStatus.value = 'Error';
    alert('Could not access microphone. Please check permissions.');
  }
};

const requestScreenShare = async () => {
  if (screenStream.value) return;
  console.log('Requesting screen share access...');
  try {
    const stream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: false });
    console.log('Screen share access granted.');

    // Handle user stopping sharing via browser UI
    stream.getVideoTracks()[0].onended = () => {
      console.log('Screen sharing stopped by user.');
      screenStatus.value = 'Stopped by user';
      screenStream.value?.getTracks().forEach((track) => track.stop());
      screenStream.value = null;
      if (isCallActive.value) {
        console.log('Hanging up because screen share ended mid-call.');
        hangUp();
      }
    };
    
    screenStream.value = stream;
    screenStatus.value = 'Active';
  } catch (error) {
    console.error('Error accessing screen share:', error);
    screenStatus.value = 'Error';
    alert('Could not access screen share. Please check permissions.');
  }
};

// --- WebRTC Core Methods ---
const createPeerConnection = () => {
  console.log('Creating Peer Connection...');
  const pc = new RTCPeerConnection(peerConnectionConfig);

  pc.ontrack = (event) => {
    console.log(`Remote track received: Kind=${event.track.kind}`);
    const stream = event.streams[0];
    if (event.track.kind === 'audio' && remoteAudioElem.value) {
      remoteAudioElem.value.srcObject = stream;
    } else if (event.track.kind === 'video' && remoteVideoElem.value) {
      remoteVideoElem.value.srcObject = stream;
    }
    callStatus.value = 'Connected';
  };

  pc.onicecandidate = (event) => {
    if (event.candidate && peerConnectionTargetId) {
      console.log('Sending ICE candidate to:', peerConnectionTargetId);
      signalrConnection.value?.invoke(
        'SendIceCandidate',
        peerConnectionTargetId,
        JSON.stringify(event.candidate)
      ).catch(err => console.error("SendIceCandidate Error: ", err));
    }
  };

  pc.oniceconnectionstatechange = () => {
    console.log('ICE Connection State:', pc.iceConnectionState);
    if (
      ['failed', 'disconnected', 'closed'].includes(pc.iceConnectionState)
    ) {
      console.warn(`ICE connection state is ${pc.iceConnectionState}. Hanging up.`);
      hangUp();
    }
  };

  // Add local tracks to the connection
  micStream.value?.getTracks().forEach((track) => pc.addTrack(track, micStream.value!));
  screenStream.value?.getTracks().forEach((track) => pc.addTrack(track, screenStream.value!));
  
  peerConnection.value = pc;
};

const startCall = async () => {
  if (!canStartCall.value) {
    alert("Cannot start call. Check connection, media, and target ID.");
    return;
  }

  peerConnectionTargetId = targetConnectionId.value;
  console.log(`Starting call to: ${peerConnectionTargetId}`);
  callStatus.value = 'Calling...';
  
  createPeerConnection();

  try {
    const pc = peerConnection.value!;
    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);

    console.log('Sending offer to:', peerConnectionTargetId);
    await signalrConnection.value?.invoke(
      'SendOffer',
      peerConnectionTargetId,
      pc.localDescription?.sdp
    );
  } catch (error) {
    console.error('Error creating or sending offer:', error);
    callStatus.value = 'Offer Failed';
    hangUp();
  }
};

const hangUp = () => {
  console.log("Hanging up call and resetting state.");
  
  // Close peer connection
  if (peerConnection.value) {
    peerConnection.value.close();
    peerConnection.value = null;
  }

  // Stop and release media streams
  if (micStream.value) {
    micStream.value.getTracks().forEach(track => track.stop());
    micStream.value = null;
  }
  if (screenStream.value) {
    screenStream.value.getTracks().forEach(track => track.stop());
    screenStream.value = null;
  }

  // Clear media element sources
  if (localAudioElem.value) localAudioElem.value.srcObject = null;
  if (remoteAudioElem.value) remoteAudioElem.value.srcObject = null;
  if (remoteVideoElem.value) remoteVideoElem.value.srcObject = null;
  
  // Reset state variables
  peerConnectionTargetId = null;
  // targetConnectionId.value = ''; // Optional: clear the input
  
  // Reset statuses
  callStatus.value = 'Idle';
  micStatus.value = 'Inactive';
  screenStatus.value = 'Inactive';
};

// --- WebRTC Signal Handlers ---
async function handleOffer(senderConnectionId: string, sdpOffer: string) {
  if (isCallActive.value) {
    console.warn('Existing peer connection found when receiving offer. Ignoring.');
    return;
  }
  if (!isMicActive.value && !isScreenActive.value) {
    alert("Incoming call, but you haven't granted mic/screen access!");
    return;
  }

  console.log(`Received offer from: ${senderConnectionId}`);
  peerConnectionTargetId = senderConnectionId;
  targetConnectionId.value = senderConnectionId; // Pre-fill input
  callStatus.value = `Incoming call from ${senderConnectionId.substring(0, 8)}...`;
  
  createPeerConnection();

  try {
    const pc = peerConnection.value!;
    await pc.setRemoteDescription({ type: 'offer', sdp: sdpOffer });
    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);

    console.log('Sending answer to:', peerConnectionTargetId);
    await signalrConnection.value?.invoke(
      'SendAnswer',
      peerConnectionTargetId,
      pc.localDescription?.sdp
    );
  } catch (error) {
    console.error('Error handling offer or creating answer:', error);
    callStatus.value = 'Answer Failed';
    hangUp();
  }
}

async function handleAnswer(senderConnectionId: string, sdpAnswer: string) {
  if (!isCallActive.value) {
    console.error('Received answer but no peer connection exists.');
    return;
  }
  console.log(`Received answer from: ${senderConnectionId}`);
  try {
    await peerConnection.value!.setRemoteDescription({ type: 'answer', sdp: sdpAnswer });
    callStatus.value = 'Connecting...';
  } catch (error) {
    console.error('Error setting remote description (answer):', error);
    callStatus.value = 'Connection Failed (Answer)';
    hangUp();
  }
}

async function handleIceCandidate(senderConnectionId: string, iceCandidateJson: string) {
  if (!isCallActive.value) return;
  console.log(`Received ICE candidate from: ${senderConnectionId}`);
  try {
    const candidate = new RTCIceCandidate(JSON.parse(iceCandidateJson));
    await peerConnection.value!.addIceCandidate(candidate);
  } catch (error) {
    console.error('Error adding received ICE candidate:', error);
  }
}

// --- Lifecycle Hooks ---
onMounted(() => {
  setupSignalR();
});

onUnmounted(() => {
  hangUp(); // Ensure everything is clean
  signalrConnection.value?.stop();
});
</script>
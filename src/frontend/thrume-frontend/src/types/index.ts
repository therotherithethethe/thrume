// types.ts

// --- Re-using existing ID types ---
export interface PostId {
    value: string; // C# Guid serialized as { value: "..." }
  }
  
  export interface AccountId {
    value: string; // C# Guid serialized as { value: "..." }
  }
  
  export interface CommentId {
    value: string;
  }
  
  // --- UPDATED: RawImage now matches the new nested structure ---
  export interface RawImage {
    id: { // This 'id' is not a GUID, but an object containing the imageUrl
      imageUrl: string;
    };
    post: null; // As per your JSON
  }
  
  // --- Raw Account structure when EMBEDDED in other API responses (e.g., Post) ---
  export interface RawAccount {
    id: AccountId;
    userName: string;
    email?: string;
    pictureUrl: string | null;
  }
  
  
  // --- Raw Comment structure from API ---
  export interface RawCommentFromApi {
    id: CommentId;
    content: string;
    authorId: AccountId;
    author: RawAccount | null;
    postId: PostId;
    post: null;
    parentCommentId: CommentId | null;
    parentComment: null;
    replies: RawCommentFromApi[];
    createdAt: string;
  }
  
  // --- UPDATED: Raw Post object received directly from your API (images changed) ---
  export interface RawPostFromApi {
    id: PostId;
    content: string;
    images: RawImage[]; // Now array of RawImage objects
    authorId: AccountId;
    userName: string;
    pictureUrl: string | null;
    likedBy: AccountId[];
    createdAt: string;
    comments: RawCommentFromApi[];
  }
  
  
  // --- Raw Account object from API (for the main account store) ---
  export interface RawAccountFromApi {
    id: AccountId;
    posts: PostId[];
    likedPosts: PostId[];
    pictureUrl: string | null;
    userName: string;
    email: string;
  }
  
  
  // --- Interfaces for the ENRICHED data, suitable for PostCard consumption ---
  
  export interface Account { // Enriched Account object
    id: string;
    username: string;
    email: string;
    profilePictureUrl: string | null;
    posts?: string[];
    likedPosts?: string[];
  }
  
  export interface Comment { // Comment for PostCard, where author is resolved
    id: string;
    content: string;
    author: Account;
    createdAt: string;
  }
  
  // --- PostForCard's images remains string[] (array of URLs) ---
  export interface PostForCard { // Final Post object for PostCard
    id: string;
    content: string;
    images: string[]; // This will be an array of string URLs after transformation
    author: Account;
    likedBy: string[];
    comments: Comment[];
    createdAt: string;
  }
// --- Messaging Types ---
export interface Conversation {
  id: { value: string };
  createdAt: string;
  participants: Participant[];
}

export interface Participant {
  id: { value: string };
  userName: string;
  pictureUrl: string | null;
}
// --- Message Type ---
export interface Message {
  id: { value: string };
  conversationId: { value: string };
  senderId: { value: string };
  userName: string;
  pictureUrl: string | null;
  content: string;
  sentAt: string;
}
// --- Post Creation Types ---
export interface CreatePostRequest {
  content: string;
  images: File[];
}

// --- Profile Data Type ---
export interface ProfileData {
  accountId: string
  userName: string;
  pictureUrl: string | null;
  postCount: number;
  followersCount: number;
  followingCount: number;
  amIFollowing: boolean;
  isUserFollowingMe?: boolean;
}
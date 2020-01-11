export interface Message {
    id: string;
    senderId: string;
    senderKnownAs: string;
    senderPhotoUrl: string;
    recipientId: string;
    recipientKnownAs: string;
    recipientPhotoUrl: string;
    content: string;
    isRead: boolean;
    dateRead: Date;
    messageSent: Date;
}

import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../singalR.service';
import { String } from 'typescript-string-operations';

@Component({
  selector: 'chat-ui',
  templateUrl: './chat-ui.component.html',
  styleUrls: ['./chat-ui.component.css']
})
export class ChatUiComponent implements OnInit {

  constructor(public signalRService: SignalRService) { }
  messages = [];
  chat: Conversation;

  senderId;
  ngOnInit(): void {

  }

  initializeSignalR() {
    this.signalRService.init(this.senderId);
    this.chat = new Conversation();

    this.signalRService.messages.subscribe(data => {
      JSON.stringify(data);
      this.chat.messages.push(data as Message);
    });
    this.getConversation(this.senderId, this.toUser);
  }

  getConversation(senderPersonId, recipientPersonId) {
    let personIds = [senderPersonId, recipientPersonId];
    personIds.sort();

    const documentId = `document.conversation-${String.Join("-", personIds)}`
    const partitionKey = `partition.conversation-${String.Join("-", personIds)}`
    this.signalRService.getConversation(documentId, partitionKey).then((res) => {
      if (res != null && (res as Array<any>).length > 0)
        this.chat = res[0] as Conversation;
    })
  }

  toUser;
  message;
  SendMessage() {
    let message = new Message();
    message.content = this.message;
    message.sentOn = Date.now();
    message.recipientPersonId = this.toUser;
    message.senderPersonId = this.senderId;

    this.chat.messages.push(message);

    this.signalRService.sendMessage(this.senderId, this.toUser, JSON.stringify(message)).then(() => {
      console.log("sent");
    }).catch(err => {
      console.error(err);
    });
  }
}

export class Conversation {
  id: string;
  messages: Message[] = [];
}

export class Message {
  senderPersonId;
  recipientPersonId;
  messageId;
  content;
  sentOn;
  readOn;
}

import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../singalR.service';
import { String } from 'typescript-string-operations';
import { SharedService } from '../../shared/services/shared.service';

@Component({
  selector: 'chat-ui',
  templateUrl: './chat-ui.component.html',
  styleUrls: ['./chat-ui.component.css']
})
export class ChatUiComponent implements OnInit {

  loggedInUserB2CId;
  constructor(public signalRService: SignalRService, public sharedService: SharedService) { }

  promises: Promise<any>[] = [];

  messages = [];
  chat: Conversation;

  ngOnInit(): void {
    this.chat = new Conversation();
    this.promises.push(this.getLoggedInUser());

    Promise.all(this.promises).then(_ => {
      this.getContacts();
      this.initializeSignalR();
      alert("signalR Initialized");
    })
  }

  initializeSignalR() {
    this.signalRService.init(this.loggedInUserB2CId);

    this.signalRService.messages.subscribe(data => {
      this.chat.messages.push(data as Message);
    });
    
  }

  recipient;
  selectContact(item) {
    this.recipient = item;
    this.getConversation(this.loggedInUserB2CId, this.recipient.b2CObjectId);
  }

  contacts = [];
  getContacts() {
    this.signalRService.getContacts().then((res) => {
      this.contacts = res as [];
      this.contacts = this.contacts.filter(x => x.b2CObjectId != this.loggedInUser.b2CObjectId);
    })
  }

  loggedInUser;
  getLoggedInUser() {
    return this.sharedService.getLoggedInUser().then((res) => {
      this.loggedInUser = res;
      this.loggedInUserB2CId = res.b2CObjectId
    })
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

  message;
  SendMessage() {
    let message = new Message();
    message.content = this.message;
    message.sentOn = new Date();
    message.sender.Name = this.loggedInUser.fullName;
    message.sender.personId = this.loggedInUserB2CId;
    message.recipient.Name = this.recipient.fullName;
    message.recipient.personId = this.recipient.b2CObjectId;
    this.chat.messages.push(message);

    this.message = "";

    this.signalRService.sendMessage(this.loggedInUserB2CId, this.recipient.b2CObjectId, JSON.stringify(message)).then(() => {
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
  sender: Person = new Person;
  recipient: Person = new Person;
  messageId;
  content;
  sentOn;
  readOn;
}

export class Person {
  personId;
  Name;
}

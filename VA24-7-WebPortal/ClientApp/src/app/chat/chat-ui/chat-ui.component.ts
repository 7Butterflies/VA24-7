import { Component, OnInit } from '@angular/core';
import { SignalRService } from '../singalR.service';

@Component({
  selector: 'chat-ui',
  templateUrl: './chat-ui.component.html',
  styleUrls: ['./chat-ui.component.css']
})
export class ChatUiComponent implements OnInit {

  constructor(public signalRService: SignalRService) { }
  messages = [];

  userId;
  ngOnInit(): void {
    
  }

  initializeSignalR(){
    this.signalRService.init(this.userId);
    this.signalRService.messages.subscribe(data => {
      this.messages.push(data);
    });
  }

  username;
  message;
  SendMessage() {
    this.signalRService.sendMessage(this.username, this.message).then(() => {
      console.log("sent");
    }).catch(err => {
      console.error(err);
    });
  }

}

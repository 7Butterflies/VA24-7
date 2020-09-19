import { Component, OnInit } from '@angular/core';
import { SharedService } from './shared/services/shared.service';
import { Person } from './chat/chat-ui/chat-ui.component';
import { SignalRService } from './shared/services/signal-r.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

  constructor(private sharedService: SharedService, private signalRService: SignalRService) {

  }
  title = 'app';

  ngOnInit(): void {
    console.log("app component loaded");
  }
}


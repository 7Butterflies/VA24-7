import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatUiComponent } from './chat-ui/chat-ui.component';
import { FormsModule } from '@angular/forms';
import { ChatRoutingModule } from './chat-routing.module';

@NgModule({
  declarations: [ChatUiComponent],
  imports: [
    CommonModule,
    FormsModule,
    ChatRoutingModule
  ]
})
export class ChatModule { }

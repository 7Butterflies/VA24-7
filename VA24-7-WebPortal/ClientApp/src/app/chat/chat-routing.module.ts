import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatUiComponent } from './chat-ui/chat-ui.component';
import { Routes, RouterModule } from '@angular/router';

const routes: Routes = [
  {
    path: 'chat', component: ChatUiComponent,
  },

];
@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class ChatRoutingModule { }

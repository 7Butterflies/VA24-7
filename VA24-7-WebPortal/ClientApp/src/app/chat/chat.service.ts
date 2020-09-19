import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { SignalRConnection } from '../model/signaR-connection-model';

@Injectable({
  providedIn: 'root'
})

export class ChatService {
  messages: Subject<any> = new Subject();

  constructor(private http: HttpClient) {
  }

  sendMessage(senderId, recipientPersonId, body) {
    return this.http.post(`${environment.apiBaseUrl}/sendMessage/${senderId}/${recipientPersonId}`, body).toPromise();
  }

  getConversation(id, partitionKey) {
    return this.http.get(`${environment.apiBaseUrl}/conversations/${id}/${partitionKey}`).toPromise();
  }

  getContacts(id?) {
    return this.http.get(`${environment.apiBaseUrl}/person/${id ? id : ''}`).toPromise();
  }

}

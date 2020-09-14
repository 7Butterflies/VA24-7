import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import * as SignalR from '@aspnet/signalr';
import { SignalRConnection } from '../model/signaR-connection-model';

@Injectable({
  providedIn: 'root'
})

export class SignalRService {
  messages: Subject<string> = new Subject();

  private hubConnection: SignalR.HubConnection;
  constructor(private http: HttpClient) {
  }

  private getSignalRConnection(userId: string): Observable<SignalRConnection> {
    return this.http.get<SignalRConnection>(`${environment.apiBaseUrl}/connection/${userId}`);
  }

  init(userId: string) {
    this.getSignalRConnection(userId).subscribe(con => {
      const options = {
        accessTokenFactory: () => con.accessToken
      };

      this.hubConnection = new SignalR.HubConnectionBuilder()
        .withUrl(con.url, options)
        .configureLogging(SignalR.LogLevel.Information)
        .build();

      // this.addUserToGroup();

      this.hubConnection.on('newMessage', data => {
        this.messages.next(data);
      });


      this.hubConnection.start().then(() => {

      })
        .catch(error => console.error(error));

      this.hubConnection.serverTimeoutInMilliseconds = 300000;
      this.hubConnection.keepAliveIntervalInMilliseconds = 300000;

      this.hubConnection.onclose((error) => {
        this.hubConnection.start();
        console.error(`Something went wrong: ${error}`);
      });
    });
  }

  sendMessage(user, message) {
    return this.http.post(`${environment.apiBaseUrl}/sendMessage/${user}`, JSON.stringify({ "message": message })).toPromise();
  }

  addUserToGroup() {
    return this.hubConnection.invoke("AddUserToGroup", "user1", "group1");
  }
}

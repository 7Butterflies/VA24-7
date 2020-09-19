import { Injectable } from '@angular/core';
import { SignalRConnection } from '../../model/signaR-connection-model';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiService } from './api.service';
import * as SignalR from '@aspnet/signalr';
import { release } from 'os';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  constructor(private apiService: ApiService) { }

  private getSignalRConnection(userId: string, hubName: string): Observable<SignalRConnection> {
    return this.apiService.get<SignalRConnection>(`${environment.apiBaseUrl}/connection/${userId}/${hubName}`);
  }

  init(userId: string, hubname: string) {
    let hubConnection: SignalR.HubConnection;
    return this.getSignalRConnection(userId, hubname).toPromise().then(con => {
      const options = {
        accessTokenFactory: () => con.accessToken
      };

      hubConnection = new SignalR.HubConnectionBuilder()
        .withUrl(con.url, options)
        .configureLogging(SignalR.LogLevel.Information)
        .build();

      hubConnection.serverTimeoutInMilliseconds = 300000;
      hubConnection.keepAliveIntervalInMilliseconds = 300000;

      return hubConnection
    });
  }
}

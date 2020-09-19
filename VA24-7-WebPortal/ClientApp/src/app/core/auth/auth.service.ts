/// <summary>
///  This service is responsible for authenticating the user using the MSAL library.
/// </summary>

import { Injectable } from '@angular/core';
import { ApiService } from '../../shared/services/api.service';
import { String } from 'typescript-string-operations';
import { environment } from '../../../environments/environment';
import { SharedService } from '../../shared/services/shared.service';
import { MsalService } from '@azure/msal-angular';
import { SignalRService } from '../../shared/services/signal-r.service';

@Injectable()
export class AuthService {
  public authenticated: boolean = false;
  loggedInUser;

  constructor(private msalService: MsalService, private sharedService: SharedService, private signalRService: SignalRService) {
    if ((localStorage.getItem('msal.idtoken') != null && localStorage.getItem('msal.idtoken') != '')) {
      this.authenticated = true;
    }
  }

  public async signOut() {
    this.msalService.logout();
  }

  public async signIn() {
    this.msalService.loginRedirect();
  }
}

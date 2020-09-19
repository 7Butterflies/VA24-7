import { Component } from '@angular/core';
import { SharedService } from './shared/services/shared.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {

  constructor(private sharedService: SharedService) {
    const token = localStorage.getItem('msal.idtoken');
    if (token) {
      this.sharedService.getOrCreateLoggedInUser();
    }
  }
  title = 'app';
}


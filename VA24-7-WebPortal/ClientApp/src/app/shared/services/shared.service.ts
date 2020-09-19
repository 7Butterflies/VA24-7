import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SharedService {

  constructor(private apiService: ApiService) { }

  getOrCreateLoggedInUser() {
    return this.apiService.post(`${environment.apiBaseUrl}/person`, JSON.stringify({})).toPromise();
  }
}

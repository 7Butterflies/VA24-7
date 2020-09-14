/// <summary>
///  This Class will intercept all the outgoing api requests and adds the bearer token to the request header.
/// </summary>

import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable()
export class HttpTokenInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const tokens = sessionStorage.getItem('tokens') != null && JSON.parse(sessionStorage.getItem('tokens'));

    //Dont add the Authorization header while refreshing the token
    if (tokens["id_token"] != null && tokens["id_token"] != '') {
      //Refresh the token 5 mins prior its expiration time, so we don't ran into session timeout issue.
      req = req.clone({ headers: req.headers.set('Authorization', 'Bearer ' + tokens["id_token"]) });
    }

   

    return next.handle(req);
  }
}

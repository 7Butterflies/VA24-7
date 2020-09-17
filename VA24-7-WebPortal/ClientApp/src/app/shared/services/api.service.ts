/// <summary>
/// This service works as a wrapper class for all the http metthods.
/// </summary>

import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  constructor(private http: HttpClient) { }

  get<T>(url: string, responseType?: any): Observable<any> {
    if (responseType != null)
      return this.http.get<T>(url, responseType);

    return this.http.get<T>(url);
  }

  post<T>(url: string, body: any, headers?: any): Observable<any> {
    if (headers)
      return this.http.post<T>(url, body, { headers: headers });
    else
      return this.http.post<T>(url, body);
  }

   postWithRepsoneTypeAndHeaders<T>(url: string, body: any,headers?: any, responseType?: any): Observable<any> {
    if (responseType)
      return this.http.post<T>(url, body, {headers, responseType: responseType});
    else
      return this.http.post<T>(url, body);
  }

  put<T>(url: string, body: string): Observable<T> {
    return this.http.put<T>(url, body);
  }

  delete<T>(url: string, body?: string): Observable<T> {
    if (body)
      return this.http.request<T>("delete", url, { body: body });

    return this.http.delete<T>(url);
  }

  patch<T>(url: string, body: string): Observable<T> {
    return this.http.patch<T>(url, body);
  }
}

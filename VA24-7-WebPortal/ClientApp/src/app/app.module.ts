import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { Configuration } from 'msal';
import {
  MsalModule,
  MSAL_CONFIG,
  MSAL_CONFIG_ANGULAR,
  MsalService,
  MsalAngularConfiguration} from '@azure/msal-angular';

import { msalConfig, msalAngularConfig } from './msal-config';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { ChatModule } from './chat/chat.module';
import { SharedModule } from './shared/shared.module';
import { CoreModule } from './core/core.module';

export function MSALConfigFactory(): Configuration {
  return msalConfig;
}

export function MSALAngularConfigFactory(): MsalAngularConfiguration {
  return msalAngularConfig;
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ChatModule,
    SharedModule,
    CoreModule,
    MsalModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full'},
    ])
  ],
  providers: [
    {
      provide: MSAL_CONFIG,
      useFactory: MSALConfigFactory
    },
    {
      provide: MSAL_CONFIG_ANGULAR,
      useFactory: MSALAngularConfigFactory
    },
    MsalService],
  bootstrap: [AppComponent]
})
export class AppModule { }

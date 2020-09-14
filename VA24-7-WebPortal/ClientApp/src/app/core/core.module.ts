/// <summary>
///  This module shares the singletom services and app level components.The services defined in the Core Module are instantiated once.
/// </summary>
/// <remarks>
///  This type of module is imported only from the main module, as it contains singleton services that any element in the application can use.
/// <remarks>

import { NgModule } from '@angular/core';

import { RouterModule } from '@angular/router';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { AuthService } from './auth/auth.service';
import { SharedModule } from '../shared/shared.module';
import { ApiService } from '../shared/services/api.service';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpTokenInterceptor } from './auth/http-token-interceptor';



@NgModule({
    declarations: [NavMenuComponent],
    imports: [
      RouterModule,
      SharedModule,
    ],
  providers: [AuthService, ApiService, {
    provide: HTTP_INTERCEPTORS,
    useClass: HttpTokenInterceptor,
    multi: true
  }],
    exports: [NavMenuComponent],
})
export class CoreModule { }

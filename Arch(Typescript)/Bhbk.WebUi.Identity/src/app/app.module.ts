import { AppComponent } from 'src/app/app.component';
import { AppRoutingModule } from 'src/app/app-routing.module';
import { JwtInterceptor } from 'src/app/common/misc';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NotificationModule } from '@progress/kendo-angular-notification';
import { IdentityAdminService, IdentityMeService, IdentityStsService } from 'src/app/common/services';
import { CommonHomeComponent } from 'src/app/common-home/common-home.component';

@NgModule({
  declarations: [
    AppComponent,
    CommonHomeComponent,
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    NotificationModule,
  ],
  providers: [
    IdentityAdminService,
    IdentityMeService,
    IdentityStsService,
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
  ],
  bootstrap: [
    AppComponent
  ],
})
export class AppModule { }

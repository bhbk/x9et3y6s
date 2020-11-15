import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonHomeComponent } from 'src/app/common-home/common-home.component';

const routes: Routes = [
  { path: 'admin', loadChildren: './identity-admin/identity-admin.module#IdentityAdminModule' },
  { path: 'me', loadChildren: './identity-me/identity-me.module#IdentityMeModule' },
  { path: 'home', component: CommonHomeComponent },
  { path: '**', pathMatch: "full", redirectTo: '/home' },
];
 
@NgModule({
  imports: [
    RouterModule.forRoot(routes),
  ],
  exports: [
    RouterModule,
  ]
})
export class AppRoutingModule { }

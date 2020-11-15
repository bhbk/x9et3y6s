import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { 
  IdentityMeAuthorizeComponent,
  IdentityMeAuthorizeCallbackComponent,
} from 'src/app/identity-me';

const routes: Routes = [
  { path: '',
    children: [
      { path: 'authorize', component: IdentityMeAuthorizeComponent },
      { path: 'authorize-callback', component: IdentityMeAuthorizeCallbackComponent },
      { path: '**', pathMatch: "full", redirectTo: '/home' },
    ]}
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
  ],
  exports: [
    RouterModule,
  ]
})
export class IdentityMeRoutingModule { }

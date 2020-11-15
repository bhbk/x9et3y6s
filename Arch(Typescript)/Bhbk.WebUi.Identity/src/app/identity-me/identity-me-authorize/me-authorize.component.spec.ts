import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityMeAuthorizeComponent } from 'src/app/identity-me';

describe('IdentityMeAuthorizeComponent', () => {
  let component: IdentityMeAuthorizeComponent;
  let fixture: ComponentFixture<IdentityMeAuthorizeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityMeAuthorizeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityMeAuthorizeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

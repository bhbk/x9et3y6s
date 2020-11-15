import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CommonHomeComponent } from './common-home.component';

describe('CommonHomeComponent', () => {
  let component: CommonHomeComponent;
  let fixture: ComponentFixture<CommonHomeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CommonHomeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CommonHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

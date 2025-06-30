import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LibIdentity } from './lib-identity';

describe('LibIdentity', () => {
  let component: LibIdentity;
  let fixture: ComponentFixture<LibIdentity>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LibIdentity]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LibIdentity);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

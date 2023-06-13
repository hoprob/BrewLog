import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LogNoteComponent } from './log-note.component';

describe('LogNoteComponent', () => {
  let component: LogNoteComponent;
  let fixture: ComponentFixture<LogNoteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LogNoteComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LogNoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

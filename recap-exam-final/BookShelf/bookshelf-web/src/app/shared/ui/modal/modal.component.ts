import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html'
})
export class ModalComponent {
  @Input() title = '';
  @Input() description = '';
  @Input() saveLabel = 'Save';
  @Input() cancelLabel = 'Cancel';
  @Input() busy = false;

  @Output() cancel = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<void>();

  onBackdropClick() { if (!this.busy) this.cancel.emit(); }
  onCancelClick() { if (!this.busy) this.cancel.emit(); }
  onConfirmClick() { if (!this.busy) this.confirm.emit(); }
}

import { process } from '@progress/kendo-data-query';
import { NgForm } from '@angular/forms';

export abstract class NgGridBase<T> {
  gridData: T[];
  protected gridItem: T;

  addHandler(event): void {
    event.sender.addRow({});
  }

  cancelHandler(event): void {
    if (this.gridItem) {
      Object.assign(event.dataItem, this.gridItem);
    }
    event.sender.closeRow(event.rowIndex);
    this.gridItem = null;
  }

  editHandler(event): void {
    this.gridItem = Object.assign({}, event.dataItem);
    event.sender.editRow(event.rowIndex);
  }

  abstract removeHandler(event): void;

  abstract saveHandler(event, form: NgForm): void;

  protected updateState(sender): void {
    sender.data = process(this.gridData, {
      filter: sender.filter,
      skip: sender.skip,
      take: sender.pageSize,
      sort: sender.sort
    });
  }
}

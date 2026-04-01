import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-prime-tree-table',
  templateUrl: './prime-tree-table.component.html',
  styleUrl: './prime-tree-table.component.scss'
})
export class PrimeTreeTableComponent {


  
  /* ================= INPUTS ================= */

  private _data: any[] = [];
  private _expandAll = false;

  @Input()
  set data(value: any[]) {
    this._data = value || [];
    this.syncExpandState();
  }
  get data(): any[] {
    return this._data;
  }

  @Input()
  set expandAll(value: boolean) {
    this._expandAll = value;
    // console.log('_expandAll: ', this._expandAll)
    this.syncExpandState();
  }
  get expandAll(): boolean {
    return this._expandAll;
  }

  @Input() groupedColumns: any[] = [];
  @Input() firstColumnField: string = 'role';
  @Input() firstColumnHeader: string = 'role';
  @Input() isLoading: boolean = false;

  /* ================= OUTPUTS ================= */

  @Output() iconClicked = new EventEmitter<any>();
  @Output() numberClick = new EventEmitter<{ row: any; prefix: string; sub: string; value: number }>();
  @Output() iconClick = new EventEmitter<any>();

  /* ================= LOGIC ================= */

  private syncExpandState() {
    if (!this._data?.length) return;

    this.normalizeChildren(this._data);

    this._expandAll
      ? this.expandNodes(this._data)
      : this.collapseNodes(this._data);

    // Force change detection by new reference
    this._data = [...this._data];
  }

  private expandNodes(nodes: any[]) {
    nodes.forEach(node => {
      node.expanded = true;
      if (Array.isArray(node.children)) {
        this.expandNodes(node.children);
      }
    });
  }

  private collapseNodes(nodes: any[]) {
    nodes.forEach(node => {
      node.expanded = false;
      if (Array.isArray(node.children)) {
        this.collapseNodes(node.children);
      }
    });
  }

  private normalizeChildren(nodes: any[]) {
    nodes.forEach(node => {
      if (!Array.isArray(node.children)) {
        node.children = [];
      } else {
        this.normalizeChildren(node.children);
      }
    });
  }

  /* ================= UI HELPERS (UNCHANGED) ================= */

  getHeaderClass(subField: any): string {
    switch (subField.label) {
      case 'Bank book total': return 'count-future';
      case 'Bank slip total': return 'count-overdue';
      case 'future': return 'count-future';
      default: return '';
    }
  }

  negativeAmtfield(amt: any): string {
    return amt?.toString().startsWith('-') ? 'count-overdue' : '';
  }

  amountField(amt: any): string {
    if (amt == null) return '';
    return amt.toString().startsWith('-') ? 'count-overdue' : 'blue-theme';
  }

  getSubHeaderClass(subField: any): string {
    const label = subField.label?.trim();
    switch (label) {
      case 'Balance as per bank':
      case 'Balance as per bank (Refreshed @ 15/12/2025 15:48:20)':
      case 'Balance(Bank Book)': return 'blue-theme';

      case 'Reco date':
      case 'Last Refreshed Date': return 'header-future';

      case 'Closing Balance(Bank Stmt)': return 'header-today';
      default: return '';
    }
  }

  isRupeeField(field: string): boolean {
    return [
      'closing_balance_as_per_bank_statement',
      'current_account_balance_as_per_bank_book',
      'balAvailable'
    ].includes(field);
  }

  onNumberClick(row: any, prefix: string, sub: string, value: number) {
    this.numberClick.emit({ row, prefix, sub, value });
  }

  get totalColspan(): number {
    return 1 + (this.groupedColumns?.reduce((acc, g) => acc + g.subFields.length, 0) || 0);
  }

  groupWidth(group: any) {
    return group.subFields.reduce((sum: number, sub: any) => sum + (sub.width || 2), 0);
  }

  onIconClick(rowData: any) {
    this.iconClicked.emit(rowData);
  }
}

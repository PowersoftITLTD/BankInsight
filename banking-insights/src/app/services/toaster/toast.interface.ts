import { ToastType } from '../toaster/toast.type';

export interface Toast {
  type: ToastType;
  title: string;
  body: string;
  delay: number;
}
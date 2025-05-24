import keyboard
import pygetwindow as gw
import win32gui
import tkinter as tk
from tkinter import ttk, messagebox, filedialog
from threading import Thread
import os
import json
from datetime import datetime
import subprocess

# Crimson theme colors
COLORS = {
    'bg': '#1a1a1a',
    'fg': '#e0e0e0',
    'button_bg': '#2d1a1a',
    'button_fg': '#ff6b6b',
    'entry_bg': '#2d1a1a',
    'entry_fg': '#ff6b6b',
    'accent': '#8b0000',
    'title': '#ff4444',
    'subtitle': '#ff6b6b',
    'text': '#cccccc',
    'tab_bg': '#2d1a1a',
    'tab_fg': '#ff6b6b',
    'selected_tab_bg': '#3d2a2a',
    'selected_tab_fg': '#ff4444',
    'list_bg': '#2d1a1a',
    'list_fg': '#ff6b6b',
    'list_select_bg': '#8b0000',
    'list_select_fg': '#ff4444'
}

class TaskManager:
    def __init__(self, parent):
        self.frame = ttk.Frame(parent, style="Crimson.TFrame")
        self.tasks = []
        self.load_tasks()
        self.setup_ui()
        
    def setup_ui(self):
        # Task entry
        entry_frame = ttk.Frame(self.frame, style="Crimson.TFrame")
        entry_frame.pack(fill=tk.X, padx=10, pady=5)
        
        self.task_var = tk.StringVar()
        self.task_entry = tk.Entry(entry_frame,
                                 textvariable=self.task_var,
                                 font=("Segoe UI", 11),
                                 bg=COLORS['entry_bg'],
                                 fg=COLORS['entry_fg'],
                                 insertbackground=COLORS['entry_fg'],
                                 relief="flat",
                                 highlightthickness=1,
                                 highlightbackground=COLORS['accent'],
                                 highlightcolor=COLORS['title'])
        self.task_entry.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=(0, 5))
        
        add_btn = ttk.Button(entry_frame,
                           text="Add Task",
                           command=self.add_task,
                           style="Crimson.TButton")
        add_btn.pack(side=tk.RIGHT)
        
        # Task list
        self.task_list = tk.Listbox(self.frame,
                                  bg=COLORS['entry_bg'],
                                  fg=COLORS['entry_fg'],
                                  selectbackground=COLORS['accent'],
                                  selectforeground=COLORS['title'],
                                  font=("Segoe UI", 11),
                                  relief="flat",
                                  highlightthickness=1,
                                  highlightbackground=COLORS['accent'])
        self.task_list.pack(fill=tk.BOTH, expand=True, padx=10, pady=5)
        
        # Buttons frame
        btn_frame = ttk.Frame(self.frame, style="Crimson.TFrame")
        btn_frame.pack(fill=tk.X, padx=10, pady=5)
        
        delete_btn = ttk.Button(btn_frame,
                              text="Delete Task",
                              command=self.delete_task,
                              style="Crimson.TButton")
        delete_btn.pack(side=tk.LEFT, padx=5)
        
        clear_btn = ttk.Button(btn_frame,
                             text="Clear All",
                             command=self.clear_tasks,
                             style="Crimson.TButton")
        clear_btn.pack(side=tk.LEFT, padx=5)
        
        # Bind Enter key
        self.task_entry.bind('<Return>', lambda e: self.add_task())
        
    def add_task(self):
        task = self.task_var.get().strip()
        if task:
            self.tasks.append(task)
            self.task_list.insert(tk.END, task)
            self.task_var.set("")
            self.save_tasks()
            
    def delete_task(self):
        selection = self.task_list.curselection()
        if selection:
            index = selection[0]
            self.task_list.delete(index)
            self.tasks.pop(index)
            self.save_tasks()
            
    def clear_tasks(self):
        self.task_list.delete(0, tk.END)
        self.tasks.clear()
        self.save_tasks()
        
    def save_tasks(self):
        with open("tasks.json", "w") as f:
            json.dump(self.tasks, f)
            
    def load_tasks(self):
        try:
            with open("tasks.json", "r") as f:
                self.tasks = json.load(f)
        except FileNotFoundError:
            self.tasks = []

class KeyEntryDialog:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Crymson - License Key Entry")
        self.root.geometry("400x200")
        self.root.configure(bg=COLORS['bg'])
        self.root.resizable(False, False)
        
        # Center the window
        self.root.eval('tk::PlaceWindow . center')
        
        # Configure style
        style = ttk.Style()
        style.configure("Crimson.TFrame", background=COLORS['bg'])
        style.configure("Crimson.TLabel", 
                       background=COLORS['bg'],
                       foreground=COLORS['text'],
                       font=("Segoe UI", 11))
        style.configure("Crimson.TButton",
                       background=COLORS['button_bg'],
                       foreground=COLORS['button_fg'],
                       padding=10,
                       font=("Segoe UI", 10, "bold"))
        style.configure("Title.TLabel",
                       background=COLORS['bg'],
                       foreground=COLORS['title'],
                       font=("Segoe UI", 18, "bold"))
        style.configure("Subtitle.TLabel",
                       background=COLORS['bg'],
                       foreground=COLORS['subtitle'],
                       font=("Segoe UI", 12))
        
        # Main frame
        main_frame = ttk.Frame(self.root, padding="20", style="Crimson.TFrame")
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # Title
        title_label = ttk.Label(main_frame, text="Crymson", style="Title.TLabel")
        title_label.pack(pady=(0, 5))
        
        subtitle_label = ttk.Label(main_frame, text="Enter License Key", style="Subtitle.TLabel")
        subtitle_label.pack(pady=(0, 15))
        
        # Key entry
        self.key_var = tk.StringVar()
        key_entry = tk.Entry(main_frame, 
                           textvariable=self.key_var,
                           width=40,
                           font=("Segoe UI", 11),
                           bg=COLORS['entry_bg'],
                           fg=COLORS['entry_fg'],
                           insertbackground=COLORS['entry_fg'],
                           relief="flat",
                           highlightthickness=1,
                           highlightbackground=COLORS['accent'],
                           highlightcolor=COLORS['title'])
        key_entry.pack(pady=10)
        key_entry.focus()
        
        # Submit button
        submit_btn = ttk.Button(main_frame,
                              text="Activate",
                              command=self.validate_key,
                              style="Crimson.TButton")
        submit_btn.pack(pady=10)
        
        # Bind Enter key
        self.root.bind('<Return>', lambda e: self.validate_key())
        
        self.result = None
        
    def validate_key(self):
        key = self.key_var.get().strip().upper()
        if key != "CRYMSON":
            messagebox.showerror("Invalid Key", "Please enter the correct license key.")
            return
            
        # Save the key
        with open("license_key.txt", "w") as f:
            f.write(key)
            
        self.result = True
        self.root.destroy()
        
    def show(self):
        self.root.mainloop()
        return self.result

class SettingsManager:
    def __init__(self, parent, callback):
        self.frame = ttk.Frame(parent, style="Crimson.TFrame")
        self.callback = callback
        self.current_hotkey = self.load_hotkey()
        self.setup_ui()
        
    def setup_ui(self):
        # Hotkey settings
        hotkey_frame = ttk.Frame(self.frame, style="Crimson.TFrame")
        hotkey_frame.pack(fill=tk.X, padx=10, pady=5)
        
        hotkey_label = ttk.Label(hotkey_frame,
                               text="Custom Hotkey:",
                               style="Subtitle.TLabel")
        hotkey_label.pack(side=tk.LEFT, padx=(0, 10))
        
        self.hotkey_var = tk.StringVar(value=self.current_hotkey)
        self.hotkey_entry = tk.Entry(hotkey_frame,
                                   textvariable=self.hotkey_var,
                                   font=("Segoe UI", 11),
                                   bg=COLORS['entry_bg'],
                                   fg=COLORS['entry_fg'],
                                   insertbackground=COLORS['entry_fg'],
                                   relief="flat",
                                   highlightthickness=1,
                                   highlightbackground=COLORS['accent'],
                                   highlightcolor=COLORS['title'])
        self.hotkey_entry.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=(0, 5))
        
        save_btn = ttk.Button(hotkey_frame,
                            text="Save",
                            command=self.save_hotkey,
                            style="Crimson.TButton")
        save_btn.pack(side=tk.RIGHT)
        
        # Instructions
        instructions = ttk.Label(self.frame,
                               text="Enter hotkey combination (e.g., 'ctrl+alt+m')",
                               style="Crimson.TLabel")
        instructions.pack(pady=5)
        
        # Default button
        default_btn = ttk.Button(self.frame,
                               text="Reset to Default (Ctrl+Tab)",
                               command=self.reset_to_default,
                               style="Crimson.TButton")
        default_btn.pack(pady=10)
        
    def save_hotkey(self):
        new_hotkey = self.hotkey_var.get().strip().lower()
        if not new_hotkey:
            messagebox.showerror("Error", "Please enter a valid hotkey combination.")
            return
            
        try:
            # Test if the hotkey is valid
            keyboard.parse_hotkey(new_hotkey)
            self.current_hotkey = new_hotkey
            self.save_to_file()
            self.callback(new_hotkey)
            messagebox.showinfo("Success", "Hotkey updated successfully!")
        except Exception as e:
            messagebox.showerror("Error", f"Invalid hotkey combination: {str(e)}")
            
    def reset_to_default(self):
        self.current_hotkey = "ctrl+tab"
        self.hotkey_var.set(self.current_hotkey)
        self.save_to_file()
        self.callback(self.current_hotkey)
        messagebox.showinfo("Success", "Hotkey reset to default (Ctrl+Tab)")
        
    def save_to_file(self):
        with open("settings.json", "w") as f:
            json.dump({"hotkey": self.current_hotkey}, f)
            
    def load_hotkey(self):
        try:
            with open("settings.json", "r") as f:
                settings = json.load(f)
                return settings.get("hotkey", "ctrl+tab")
        except FileNotFoundError:
            return "ctrl+tab"

class FileManager:
    def __init__(self, parent):
        self.frame = ttk.Frame(parent, style="Crimson.TFrame")
        self.batch_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "run_file_manager.bat")
        self.setup_ui()
        
    def setup_ui(self):
        # Only one button to open the file manager
        run_btn = ttk.Button(self.frame,
                           text="Open File Manager",
                           command=self.run_file_manager,
                           style="Crimson.TButton")
        run_btn.pack(pady=20)
        
        # Status
        self.status_var = tk.StringVar(value="Ready")
        status_label = ttk.Label(self.frame,
                               textvariable=self.status_var,
                               style="Crimson.TLabel")
        status_label.pack(pady=5)
        
    def run_file_manager(self):
        if not os.path.exists(self.batch_path):
            messagebox.showerror("Error", "run_file_manager.bat not found in the application folder.")
            self.status_var.set("File Manager Not Found")
            return
        try:
            # Hide the command prompt window
            startupinfo = subprocess.STARTUPINFO()
            startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            startupinfo.wShowWindow = subprocess.SW_HIDE
            subprocess.Popen(
                self.batch_path,
                shell=True,
                startupinfo=startupinfo,
                creationflags=subprocess.CREATE_NO_WINDOW
            )
            self.status_var.set("File Manager Started")
        except Exception as e:
            messagebox.showerror("Error", f"Failed to run file manager: {str(e)}")
            self.status_var.set("Error: Failed to start")

class MinimizeApp:
    def __init__(self):
        self.hotkey_active = False
        self.own_title = "Crymson"
        self.current_hotkey = "ctrl+tab"
        self.hotkey_thread = None
        
        # Show key entry dialog first
        key_dialog = KeyEntryDialog()
        if not key_dialog.show():
            return
            
        self.setup_ui()
        self.hotkey_active = True
        self.status_var.set("Hotkey: ON")
        self.toggle_btn.config(state="normal")
        self.start_hotkey_listener()
    
    def setup_ui(self):
        self.root = tk.Tk()
        self.root.title(self.own_title)
        self.root.geometry("500x400")
        self.root.configure(bg=COLORS['bg'])
        
        # Configure style
        style = ttk.Style()
        style.configure("Crimson.TFrame", background=COLORS['bg'])
        style.configure("Crimson.TLabel",
                       background=COLORS['bg'],
                       foreground=COLORS['text'],
                       font=("Segoe UI", 11))
        style.configure("Crimson.TButton",
                       background=COLORS['button_bg'],
                       foreground=COLORS['button_fg'],
                       padding=10,
                       font=("Segoe UI", 10, "bold"))
        style.configure("Title.TLabel",
                       background=COLORS['bg'],
                       foreground=COLORS['title'],
                       font=("Segoe UI", 24, "bold"))
        style.configure("Subtitle.TLabel",
                       background=COLORS['bg'],
                       foreground=COLORS['subtitle'],
                       font=("Segoe UI", 12))
        style.configure("Crimson.TNotebook",
                       background=COLORS['bg'],
                       borderwidth=0)
        style.configure("Crimson.TNotebook.Tab",
                       background=COLORS['tab_bg'],
                       foreground=COLORS['tab_fg'],
                       padding=[10, 5],
                       font=("Segoe UI", 10, "bold"))
        style.map("Crimson.TNotebook.Tab",
                 background=[("selected", COLORS['selected_tab_bg'])],
                 foreground=[("selected", COLORS['selected_tab_fg'])])
        
        # Main frame
        main_frame = ttk.Frame(self.root, padding="20", style="Crimson.TFrame")
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # Title
        title_label = ttk.Label(main_frame, text="Crymson", style="Title.TLabel")
        title_label.pack(pady=(0, 5))
        
        # Notebook (Tabs)
        notebook = ttk.Notebook(main_frame, style="Crimson.TNotebook")
        notebook.pack(fill=tk.BOTH, expand=True, pady=10)
        
        # Minimizer Tab
        minimizer_frame = ttk.Frame(notebook, style="Crimson.TFrame")
        notebook.add(minimizer_frame, text="Minimizer")
        
        # Status
        self.status_var = tk.StringVar(value="Hotkey: OFF")
        status_label = ttk.Label(minimizer_frame, textvariable=self.status_var, style="Subtitle.TLabel")
        status_label.pack(pady=10)
        
        # Hotkey info
        self.hotkey_info = ttk.Label(minimizer_frame,
                              text="Press Ctrl+Tab to minimize all windows",
                              style="Crimson.TLabel")
        self.hotkey_info.pack(pady=5)
        
        # Toggle button
        self.toggle_btn = ttk.Button(minimizer_frame,
                                   text="Toggle Hotkey",
                                   command=self.toggle_hotkey,
                                   style="Crimson.TButton")
        self.toggle_btn.pack(pady=10)
        self.toggle_btn.config(state="disabled")
        
        # Tasks Tab
        tasks_frame = ttk.Frame(notebook, style="Crimson.TFrame")
        notebook.add(tasks_frame, text="Tasks")
        self.task_manager = TaskManager(tasks_frame)
        self.task_manager.frame.pack(fill=tk.BOTH, expand=True)
        
        # Add Settings Tab after Tasks Tab
        settings_frame = ttk.Frame(notebook, style="Crimson.TFrame")
        notebook.add(settings_frame, text="Settings")
        self.settings_manager = SettingsManager(settings_frame, self.update_hotkey)
        self.settings_manager.frame.pack(fill=tk.BOTH, expand=True)
        
        # Files Tab
        files_frame = ttk.Frame(notebook, style="Crimson.TFrame")
        notebook.add(files_frame, text="Files")
        self.file_manager = FileManager(files_frame)
        self.file_manager.frame.pack(fill=tk.BOTH, expand=True)
        
        # Version info
        version_label = ttk.Label(main_frame,
                                text="Version 1.0",
                                style="Crimson.TLabel")
        version_label.pack(side=tk.BOTTOM, pady=5)
    
    def minimize_all_windows(self):
        for w in gw.getAllTitles():
            if w and self.own_title not in w:
                hwnd = win32gui.FindWindow(None, w)
                if hwnd:
                    win32gui.ShowWindow(hwnd, 6)  # 6 = Minimize
    
    def hotkey_listener(self):
        while self.hotkey_active:
            try:
                keyboard.wait(self.current_hotkey)
                if self.hotkey_active:  # Check again after waiting
                    self.minimize_all_windows()
            except Exception as e:
                print(f"Hotkey error: {e}")
                break
    
    def toggle_hotkey(self):
        if self.hotkey_active:
            self.stop_hotkey_listener()
            self.status_var.set("Hotkey: OFF")
        else:
            self.start_hotkey_listener()
            self.status_var.set("Hotkey: ON")
        self.hotkey_active = not self.hotkey_active
    
    def start_hotkey_listener(self):
        self.hotkey_active = True
        self.hotkey_thread = Thread(target=self.hotkey_listener, daemon=True)
        self.hotkey_thread.start()
    
    def update_hotkey(self, new_hotkey):
        self.current_hotkey = new_hotkey
        self.hotkey_info.config(text=f"Press {self.current_hotkey} to minimize all windows")
        if self.hotkey_active:
            self.stop_hotkey_listener()
            self.start_hotkey_listener()
    
    def stop_hotkey_listener(self):
        self.hotkey_active = False
        if self.hotkey_thread:
            self.hotkey_thread.join(timeout=1.0)
            self.hotkey_thread = None
    
    def run(self):
        self.root.mainloop()

if __name__ == "__main__":
    app = MinimizeApp()
    app.run()
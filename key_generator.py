def generate_key():
    return "CRYMSON"

if __name__ == "__main__":
    key = generate_key()
    print(f"License Key: {key}")
    
    # Save to a file
    with open("license_key.txt", "w") as f:
        f.write(key)
    print("Key has been saved to license_key.txt")
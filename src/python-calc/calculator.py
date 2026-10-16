"""A small command-line calculator with four basic arithmetic operations."""

from typing import Callable, Dict


def add(first_number: float, second_number: float) -> float:
    """Return the sum of two numbers."""
    return first_number + second_number


def subtract(first_number: float, second_number: float) -> float:
    """Return the difference between two numbers."""
    return first_number - second_number


def multiply(first_number: float, second_number: float) -> float:
    """Return the product of two numbers."""
    return first_number * second_number


def divide(first_number: float, second_number: float) -> float:
    """Return the quotient of two numbers."""
    return first_number / second_number


OPERATIONS: Dict[str, Callable[[float, float], float]] = {
    "+": add,
    "-": subtract,
    "*": multiply,
    "/": divide,
}


def main() -> None:
    """Read one calculation from the user and display its result."""
    try:
        first_number = float(input("First number: "))
        operation = input("Operation (+, -, *, /): ").strip()
        second_number = float(input("Second number: "))

        if operation not in OPERATIONS:
            raise ValueError("Unsupported operation.")

        result = OPERATIONS[operation](first_number, second_number)
        print(f"Result: {result}")
    except (ValueError, ZeroDivisionError) as error:
        print(f"Error: {error}")


if __name__ == "__main__":
    main()
